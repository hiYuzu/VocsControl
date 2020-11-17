using System;
using System.Text;
using System.Threading;
using System.Net;
using VocsControlHELP.model;
using VocsControlHELP.util;
using VocsControlBLL.modbus;
using VocsControlHELP.Log4Net;

namespace VocsControlBLL.tcp
{
    public class TcpService
    {
        //配置文件路径
        private string configPath;
        //运行状态标志
        private bool isStart = false;
        //数据操作守护线程
        private Thread dataGuardThread;
        //数据操作线程
        private Thread dataThread;
        //异步tcp
        private AsyncTcpClient tcpClient;

        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {
            try
            {
                configPath = string.Format("{0}tcp\\config.ini", AppDomain.CurrentDomain.BaseDirectory);
                if (StartTcpClient())
                {
                    isStart = true;
                    StartOperatorData();
                    Log4NetUtil.Info("服务启动成功");
                }
            }
            catch (Exception ex)
            {
                Log4NetUtil.Error("服务启动失败，原因：" + ex.Message);
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            isStart = false;
            if (tcpClient != null && tcpClient.isConnected)
            {
                tcpClient.Close();
            }
            if (dataGuardThread != null && dataGuardThread.IsAlive)
            {
                dataGuardThread.Abort();
            }
            if (dataThread != null && dataThread.IsAlive)
            {
                dataThread.Abort();
            }
        }

        /// <summary>
        /// 启动TCP连接
        /// </summary>
        /// <returns></returns>
        private bool StartTcpClient()
        {
            bool flag = false;
            try
            {
                StringBuilder temp = new StringBuilder();
                DllUtil.ReadConfigFile("IP", "Ip", "", temp, 255, configPath);
                string ip = temp.ToString();
                temp.Clear();
                DllUtil.ReadConfigFile("PORT", "Port", "", temp, 255, configPath);
                int port = Convert.ToInt32(temp.ToString());
                temp.Clear();
                tcpClient = new AsyncTcpClient(IPAddress.Parse(ip), port);
                tcpClient.Connect();
                tcpClient.ReceiveProcess = new AsyncTcpClient.ReadDelegate(ReceiveMessage);
                flag = true;
            }
            catch (Exception ex)
            {
                Log4NetUtil.Error("TCP连接失败，原因：" + ex.Message);
                throw new Exception(ex.Message);
            }
            return flag;
        }

        /// <summary>
        /// 开启获取数据守护线程
        /// </summary>
        private void StartOperatorData()
        {
            //启动获取数据守护线程
            if (dataGuardThread == null || !dataGuardThread.IsAlive)
            {
                dataGuardThread = new Thread(new ThreadStart(GuardThread))
                {
                    Name = "DataGuardThread",
                    IsBackground = true
                };
                dataGuardThread.Start();
            }
        }

        /// <summary>
        /// 获取数据守护线程
        /// </summary>
        private void GuardThread()
        {
            while (isStart)
            {
                try
                {
                    //启动获取数据线程
                    if (dataThread == null || !dataThread.IsAlive)
                    {
                        dataThread = new Thread(new ThreadStart(DataThread))
                        {
                            Name = "DataThread",
                            IsBackground = true
                        };
                        dataThread.Start();
                    }
                }
                catch (ThreadAbortException)
                {
                    Log4NetUtil.Info("手动停止tcp守护线程");
                    break;
                }
                catch (Exception ex)
                {
                    Log4NetUtil.Error("守护线程运行错误，信息为：" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 操作获取数据线程
        /// </summary>
        private void DataThread()
        {
            while (isStart)
            {
                try
                {
                    ModbusResultModel modbusResult = ModbusRtu.ModbusResult;
                    if(modbusResult != null && modbusResult.Time != null)
                    {
                        if (SendMessage(modbusResult))
                        {
                            Thread.Sleep(30000);
                        }
                        else
                        {
                            Log4NetUtil.Info("自动重连程序启动，每3s尝试一次！");
                            ReConnect();
                        }
                    }
                    Thread.Sleep(30000);
                }
                catch (ThreadAbortException)
                {
                    Log4NetUtil.Info("手动停止tcp操作线程");
                    break;
                }
                catch (Exception ex)
                {
                    Log4NetUtil.Error(ex.GetType().ToString() + ":" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 发送字符串数据
        /// </summary>
        /// <param name="message">字符串数据</param>
        private bool SendMessage(ModbusResultModel modbusResult)
        {
            bool flag = false;
            try
            {
                if (tcpClient != null)
                {
                    if (!tcpClient.isConnected)
                    {
                        Log4NetUtil.Info("tcp连接掉线!5s后重连发送数据");
                        Thread.Sleep(5000);
                        flag = ReConnectSend(modbusResult);
                    }
                    else
                    {
                        byte[] message = ConvertMessage(modbusResult);
                        tcpClient.Write(message);
                        flag = true;
                    }
                }
                else
                {
                    Log4NetUtil.Error("未能初始化TCP对象，程序将关闭...");
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                if (tcpClient.isConnected && ReConnectSend(modbusResult))
                {
                    flag = true;
                }
                else
                {
                    string ms = ex.Message;
                    Log4NetUtil.Error("发送数据失败，原因：" + ms);
                }
            }
            return flag;
        }

        /// <summary>
        /// 重新连接后发送
        /// </summary>
        /// <param name="message">发送信息</param>
        private bool ReConnectSend(ModbusResultModel modbusResult)
        {
            try
            {
                tcpClient.Close();
                tcpClient = null;
                if (StartTcpClient())
                {
                    //等待3s
                    Thread.Sleep(3000);
                    byte[] message = ConvertMessage(modbusResult);
                    if (!tcpClient.isConnected)
                    {
                        Log4NetUtil.Info("重连失败，请检查服务器是否开启！");
                        return false;
                    }
                    tcpClient.Write(message);
                    return true;
                }
                else
                {
                    Log4NetUtil.Info("发送失败：连接断开，未能重新建立连接");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4NetUtil.Error("连接发送数据失败，错误信息位：" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 数据转换封装
        /// </summary>
        private byte[] ConvertMessage(ModbusResultModel modbusResultModel)
        {
            Encoding encoding = Encoding.Default;
            byte[] data = new byte[35];
            byte[] message = new byte[41];
            //帧头
            message[0] = 0x7d;
            message[1] = 0x7b;
            //命令码
            data[0] = 0x02;
            //命令扩展码
            data[1] = 0x66;
            //时间
            byte[] time = encoding.GetBytes(modbusResultModel.Time);
            time.CopyTo(data, 2);
            //状态
            data[21] = 0x01;
            //个数
            data[22] = 0x03;
            //甲烷
            byte[] jw = BitConverter.GetBytes(modbusResultModel.Jw);
            Array.Reverse(jw);
            jw.CopyTo(data, 23);
            //非甲烷总烃
            byte[] zt_jw = BitConverter.GetBytes(modbusResultModel.Zt - modbusResultModel.Jw);
            Array.Reverse(zt_jw);
            zt_jw.CopyTo(data, 27);
            //总烃
            byte[] zt = BitConverter.GetBytes(modbusResultModel.Zt);
            Array.Reverse(zt);
            zt.CopyTo(data, 31);
            byte[] crcVal = CRC.GetCrcData(data);
            data.CopyTo(message, 2);
            //CRC
            message[37] = crcVal[0];
            message[38] = crcVal[1];
            //帧尾
            message[39] = 0x7d;
            message[40] = 0x7d;
            return message;
        }

        /// <summary>
        /// 接收委托触发事件
        /// </summary>
        /// <param name="receiveMessage"></param>
        private void ReceiveMessage(string receiveMessage)
        {
            //接收到的信息处理
            try
            {
                if (!string.IsNullOrEmpty(receiveMessage))
                {
                    Log4NetUtil.Info("接收到信息为：" + receiveMessage);
                }
            }
            catch (Exception ex)
            {
                Log4NetUtil.Error("接收信息后处理错误，原因：" + ex.Message);
            }
        }

        /// <summary>
        /// 重连方法
        /// </summary>
        private void ReConnect()
        {
            bool reConn = false;
            int count = 1;
            while (!reConn)
            {
                Log4NetUtil.Info("第"+ count + "次tcp重连开始");
                StartTcpClient();
                Thread.Sleep(5000);
                if (tcpClient.isConnected)
                {
                    reConn = true;
                    Log4NetUtil.Info("重连成功！");
                }
                count++;
                if(count > 10)
                {
                    //为减少系统开销，设置为每30s重连一次
                    Thread.Sleep(25000);
                }
            }
        }
    }
}
