using System;
using System.IO.Ports;
using System.Threading;
using VocsControlHELP.Log4Net;

namespace VocsControlBLL.modbus
{
    public class ModbusService
    {
        //数据操作守护线程
        private Thread dataGuardThread;
        //数据操作线程
        private Thread dataThread;
        //运行状态标志
        private bool isStart = false;
        //参数
        private SerialPort port;

        public ModbusService(SerialPort port)
        {
            this.port = port;
        }

        public void StartService()
        {
            try
            {
                isStart = true;
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
            catch(Exception e)
            {
                Log4NetUtil.Error("服务启动失败，原因：" + e.Message);
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
                    Log4NetUtil.Info("手动停止modbus守护线程");
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
                    ModbusRtu.ReadHoldingRegister(port);
                    Thread.Sleep(60000);
                }
                catch (ThreadAbortException)
                {
                    Log4NetUtil.Info("手动停止modbus操作线程");
                    break;
                }
                catch (Exception ex)
                {
                    Log4NetUtil.Error(ex.GetType().ToString() + ":" + ex.Message);
                    ModbusRtu.ModbusResult = null;
                    Thread.Sleep(60000);
                }
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            isStart = false;
            if (dataGuardThread != null && dataGuardThread.IsAlive)
            {
                dataGuardThread.Abort();
            }
            if (dataThread != null && dataThread.IsAlive)
            {
                dataThread.Abort();
            }
        }
    }
}
