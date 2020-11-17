using System.Windows;
using System;
using System.Windows.Controls;
using VocsControlHELP.model;
using System.IO.Ports;
using System.Windows.Threading;
using VocsControlBLL.modbus;
using VocsControlBLL.tcp;
using VocsControlBLL.watchdog;
using VocsControlHELP.util;
using System.Windows.Navigation;
using System.Collections.Generic;

namespace VocsControlUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SerialPort port;
        //串口参数
        private string portName;
        private int baudRate;
        private Parity parity;
        private int dataBits;
        private StopBits stopBits;
        //定时器
        private DispatcherTimer timer;
        //TCP服务
        private TcpService tcp;
        //modbus服务
        private ModbusService modbus;
        //时间code
        private string time = "";
        //数据显示页
        private NavigationWindow window = null;
        //清除显示窗口计数 60次
        private ushort clearCount = 0;
        //自动启动
        private readonly bool autoStart = true;

        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitSerialPort();
            SQLiteHelper.InitSqlite();
            Heartbeat.StartService();
            AutoStart();
        }

        private void AutoStart()
        {
            if (autoStart && "读取".Equals(readBtn.Content.ToString()))
            {  
                ReadBtn_Click(null, null);
            }
        }

        /// <summary>
        /// 读取按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadBtn_Click(object sender, RoutedEventArgs e)
        {
            if ("读取".Equals(readBtn.Content.ToString()))
            {
                if (CheckData())
                {
                    //初始化串口
                    InitSerialPortParameter();
                    readBtn.Content = "停止";
                    //打开TCP连接
                    tcp = new TcpService();
                    tcp.StartService();
                    //打开Modbus连接
                    modbus = new ModbusService(port);
                    modbus.StartService();
                    //开启定时任务
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(45);
                    timer.Tick += SetMsg;
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("请输入有效数据且不得为空！");
                }
            }
            else
            {
                readBtn.Content = "读取";
                //关闭定时任务
                timer.Stop();
                //关闭TCP连接
                tcp.StopService();
                //关闭Modbus连接
                modbus.StopService();
            }
        }
        /// <summary>
        /// 清除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Items.Clear();
        }

        private void QueryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDate())
            {
                if (window != null)
                {
                    window.Close();
                }
                window = new NavigationWindow
                {
                    Source = new Uri("DataPage.xaml", UriKind.Relative)
                };
                window.Height = 700;
                window.Width = 700;
                window.ResizeMode = 0;
                window.Show();
            }
            else
            {
                MessageBox.Show("请选择起止日期！");
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDate())
            {
                List<DataModel> datas = SQLiteHelper.QueryByDateTime(DateModel.StartTime, DateModel.StopTime);
                if (Export.ExportExcel(datas))
                {
                    MessageBox.Show("导出成功！");
                }
            }
            else
            {
                MessageBox.Show("请选择起止日期！");
            }
        }

        private bool CheckDate()
        {
            if(StartDate.SelectedDate.ToString() == "" && StopDate.SelectedDate.ToString() == "")
            {      
                return false;
            }
            DateModel.StartTime = ((DateTime)StartDate.SelectedDate).ToString("yyyy-MM-dd HH:mm:ss");
            DateModel.StopTime = ((DateTime)StopDate.SelectedDate).ToString("yyyy-MM-dd HH:mm:ss");
            return true;
        }

        /// <summary>
        /// 扫描设备端口号
        /// </summary>
        private void InitSerialPort()
        {
            portCombo.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = port
                };
                portCombo.Items.Add(item);
            }
        }

        /// <summary>
        /// 验证所有数据正确输入
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            bool allInput = false;
            bool haveFunc = funcCombo.SelectedIndex != -1;
            //端口号默认为COM6
            //bool havaPort = portCombo.SelectedIndex != -1;
            bool havaPort = true;
            bool havaBaud = baudCombo.SelectedIndex != -1;
            bool havaParity = parityCombo.SelectedIndex != -1;
            bool havaData = dataCombo.SelectedIndex != -1;
            bool havaStop = stopCombo.SelectedIndex != -1;
            if(haveFunc && havaPort && havaBaud && havaParity && havaData && havaStop)
            {
                allInput = true;
            }
            return allInput;
        }

        /// <summary>
        /// 初始化串口参数
        /// </summary>
        /// <returns></returns>
        private SerialPort InitSerialPortParameter()
        {
            if(portCombo.SelectedIndex != -1)
            {
                portName = portCombo.Text;
            }
            else
            {
                portName = "COM6";
                portCombo.Text = portName;
            }
            baudRate = int.Parse(baudCombo.Text);
            switch (parityCombo.Text)
            {
                case "奇校检":
                    parity = Parity.Odd;
                    break;
                case "偶校检":
                    parity = Parity.Even;
                    break;
                case "无校检":
                    parity = Parity.None;
                    break;
                default:
                    break;
            }
            dataBits = int.Parse(dataCombo.Text);
            switch (stopCombo.Text)
            {
                case "1":
                    stopBits = StopBits.One;
                    break;
                case "2":
                    stopBits = StopBits.Two;
                    break;
                default:
                    break;
            }
            port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            return port;
        }

        /// <summary>
        /// 设置当前modbus参数
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// SetMessage
        /// </summary>
        /// <param name="msg"></param>
        private void SetMsg(object sender, EventArgs e)
        {
            ModbusResultModel modbusResult = ModbusRtu.ModbusResult;
            if (modbusResult != null && time != modbusResult.Time)
            {
                time = modbusResult.Time;
                string state = "";
                switch (modbusResult.StateId)
                {
                    case 0:
                        state = "关";
                        break;
                    case 1:
                        state = "开";
                        break;
                    case 2:
                        state = "点火中...";
                        break;
                    default:
                        break;
                }
                dataGrid.Items.Add(new DataGridRow
                {
                    Item = new
                    {
                        TimeGrid = time,
                        StateGrid = state,
                        ZtGrid = modbusResult.Zt,
                        JwGrid = modbusResult.Jw,
                        Zt_jwGrid = modbusResult.Zt - modbusResult.Jw
                    }
                });
                clearCount++;
                if(clearCount > 60)
                {
                    dataGrid.Items.Clear();
                }
            }
        }

    }
}
