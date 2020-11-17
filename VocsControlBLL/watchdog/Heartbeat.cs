using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VocsControlHELP.Log4Net;

namespace VocsControlBLL.watchdog
{
    public class Heartbeat
    {
        public static void StartService()
        {
            //RegWatchDog();
            Thread heartbeatThread = new Thread(new ThreadStart(SendHeartbeat))
            {
                Name = "HeartbeatThread",
                IsBackground = true
            };
            heartbeatThread.Start();
        }

        private static void SendHeartbeat()
        {
            IPEndPoint remoteAddr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12020);
            UdpClient udp = new UdpClient();
            byte[] sendData = Encoding.Default.GetBytes("heart_beat");
            while (true)
            {
                udp.Send(sendData, sendData.Length, remoteAddr);
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// 添加看门狗注册表
        /// </summary>
        private static void RegWatchDog()
        {
            string localPath = AppDomain.CurrentDomain.BaseDirectory + "WatchDog.exe";
            if (File.Exists(localPath))
            {
                RegistryKey reg = Registry.CurrentUser;
                try
                {
                    RegistryKey run = reg.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    //判断注册表中是否存在当前名称和值
                    if (run.GetValue("ModbusWatchDog") != null)
                    {
                        run.DeleteValue("ModbusWatchDog");
                    }
                    run.SetValue("ModbusWatchDog", localPath);
                    Log4NetUtil.Info("注册表添加成功");
                    reg.Close();
                }
                catch (Exception ex)
                {
                    Log4NetUtil.Error("注册表操作失败，失败信息：" + ex.Message);
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
