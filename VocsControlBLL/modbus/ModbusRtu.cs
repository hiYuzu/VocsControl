using System;
using System.IO.Ports;
using Modbus.Device;
using VocsControlHELP.util;
using VocsControlHELP.model;
using System.IO;
using VocsControlHELP.Log4Net;
using System.Text;

namespace VocsControlBLL.modbus
{
    public class ModbusRtu : Exception
    {
        //读取保持寄存器数组
        private static ushort[] stateBuffer;
        private static ushort[] timeBuffer;
        private static ushort[] ztBuffer;
        private static ushort[] jwBuffer;
        private static short stateId;
        private static long timeCode = 0;

        //Modbus参数
        private static byte slaveId;

        private static ushort stateAddr;
        private static ushort timeAddr;
        private static ushort ztAddr;
        private static ushort jwAddr;

        private static ushort stateLen;
        private static ushort timeLen;
        private static ushort ztjwLen;

        public static ModbusResultModel ModbusResult { set; get; }
        
        private static void ReadConfig()
        {
            string configPath;
            try
            {
                configPath = string.Format("{0}modbus\\config.ini", AppDomain.CurrentDomain.BaseDirectory);
                StringBuilder temp = new StringBuilder();

                DllUtil.ReadConfigFile("SLAVE", "Slave", "", temp, 255, configPath);
                slaveId = Convert.ToByte(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("STATE", "State", "", temp, 255, configPath);
                stateAddr = Convert.ToUInt16(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("TIME", "Time", "", temp, 255, configPath);
                timeAddr = Convert.ToUInt16(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("ZT", "Zt", "", temp, 255, configPath);
                ztAddr = Convert.ToUInt16(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("JW", "Jw", "", temp, 255, configPath);
                jwAddr = Convert.ToUInt16(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("STATELEN", "Statelen", "", temp, 255, configPath);
                stateLen = Convert.ToUInt16(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("TIMELEN", "Timelen", "", temp, 255, configPath);
                timeLen = Convert.ToUInt16(temp.ToString());
                temp.Clear();

                DllUtil.ReadConfigFile("ZTJWLEN", "Ztjwlen", "", temp, 255, configPath);
                ztjwLen = Convert.ToUInt16(temp.ToString());
                temp.Clear();
            }
            catch (Exception ex)
            {
                Log4NetUtil.Error(ex.Message);
            }
        }
        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        /// <param name="master"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static void ReadHoldingRegister(SerialPort port)
        {
            ReadConfig();
            IModbusMaster master = ModbusSerialMaster.CreateRtu(port);
            master.Transport.ReadTimeout = 1000;
            master.Transport.Retries = 3;
            master.Transport.WaitToRetryMilliseconds = 500;
            float zt;
            float jw;
            if (port.IsOpen == false)
            {
                port.Open();
            }
            //读取保持寄存器 
            timeBuffer = master.ReadHoldingRegisters(slaveId, timeAddr, timeLen);
            timeCode = GetTimeCode(timeBuffer);  
            if (SQLiteHelper.IsNewData(timeCode))
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //若时间发生变化，则查询其他从机
                stateBuffer = master.ReadHoldingRegisters(slaveId, stateAddr, stateLen);
                ztBuffer = master.ReadHoldingRegisters(slaveId, ztAddr, ztjwLen);
                jwBuffer = master.ReadHoldingRegisters(slaveId, jwAddr, ztjwLen);
                stateId = GetState(stateBuffer);
                //变量类型Float DC BA
                zt = GetData(ztBuffer);
                jw = GetData(jwBuffer);
                port.Close();
                master.Dispose();
                //插入数据库
                SQLiteHelper.InsertData(timeCode, time, stateId, zt, jw);
                ModbusResult = new ModbusResultModel(time, stateId, zt, jw);
            }
            else
            {
                port.Close();
                master.Dispose();
                Log4NetUtil.Info("查询成功，数据不需更新");
            }
        }
        private static short GetState(ushort[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            for (int i = buffer.Length - 1; i > -1; i--)
            {
                byte[] bytes = BitConverter.GetBytes(buffer[i]);
                ms.Write(bytes, 0, bytes.Length);
            }
            byte[] res = ms.GetBuffer();
            Array.Reverse(res, 0, buffer.Length * 2);
            return BitConverter.ToInt16(res, 0);
        }
        private static float GetData(ushort[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            for (int i = buffer.Length - 1; i > -1; i--)
            {
                byte[] bytes = BitConverter.GetBytes(buffer[i]);
                ms.Write(bytes, 0, bytes.Length);
            }
            byte[] res = ms.GetBuffer();
            Array.Reverse(res, 0, buffer.Length * 2);
            return BitConverter.ToSingle(res, 0);
        }
        private static long GetTimeCode(ushort[] buffer)
        {
            long timeCode = 0;
            for (int i = buffer.Length - 1; i > -1; i--)
            {
                timeCode += buffer[i];
            }
            return timeCode;
        }
    }
}