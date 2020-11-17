using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace VocsControlBLL.tcp
{
    /// <summary>
    /// 异步Tcp客户端
    /// </summary>
    public class AsyncTcpClient
    {
        private IPAddress[] addresses;
        private int port;
        private WaitHandle addressesSet;
        private TcpClient tcpClient;
        //private int failedConnectionCount;
        public bool isConnected = false;
        //定义接收信息委托
        public delegate void ReadDelegate(string receiveMsg);

        /// <summary>
        /// UDP网关内部类
        /// </summary>
        //private static class AsyncTcpHolder
        //{
        //    private static AsyncTcpClient instance = null;

        //    public static AsyncTcpClient Instance
        //    {
        //        get { return AsyncTcpHolder.instance; }
        //        set { AsyncTcpHolder.instance = value; }
        //    }
        //}

        private ReadDelegate receiveProcess = null;
        /// <summary>
        /// 处理收到的消息
        /// </summary>
        public ReadDelegate ReceiveProcess
        {
            get { return receiveProcess; }
            set { receiveProcess = value; }
        }

        /// <summary>
        /// 构造函数-IP地址
        /// </summary>
        /// <param name="address">服务器IP地址</param>
        /// <param name="port">服务器端口</param>
        public AsyncTcpClient(IPAddress address, int port)
        : this(new[] { address }, port)
        {
        }

        /// <summary>
        /// 构造函数-多IP同一个Client
        /// </summary>
        /// <param name="addresses">服务器IP地址集合</param>
        /// <param name="port">服务器端口</param>
        public AsyncTcpClient(IPAddress[] addresses, int port)
        : this(port)
        {
            this.addresses = addresses;
        }

        /// <summary>
        /// 构造函数-服务器地址（名称或地址）
        /// </summary>
        /// <param name="hostNameOrAddress">服务器地址（名称或地址）</param>
        /// <param name="port">服务端口</param>
        public AsyncTcpClient(string hostNameOrAddress, int port)
        : this(port)
        {
            addressesSet = new AutoResetEvent(false);
            Dns.BeginGetHostAddresses(hostNameOrAddress, GetHostAddressesCallback, null);
        }

        /// <summary>
        /// 其他构造函数调用的私有构造函数
        /// </summary>
        /// <param name="port">端口</param>
        private AsyncTcpClient(int port)
        {
            if (port < 0)
            {
                throw new ArgumentException();
            }
            this.port = port;
            this.tcpClient = new TcpClient();
            this.Encoding = Encoding.Default;
        }

        /// <summary>
        /// 用于接收发送数据encode/decode的编码格式
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 连接IP服务器
        /// </summary>
        public void Connect()
        {
            if (addressesSet != null)
            {
                //等待地址值被设置
                addressesSet.WaitOne();
            }
            //将失败的连接数设置为0
            //Interlocked.Exchange(ref failedConnectionCount, 0);
            //开始异步连接操作
            tcpClient.BeginConnect(addresses, port, ConnectCallback, null);
        }

        /// <summary>
        /// 默认编码格式发送字符串数据
        /// </summary>
        /// <param name="data">字符串数据</param>
        /// <returns>WaitHandle，可以用来检测当写操作完成</returns>
        public void Write(string data)
        {
            byte[] bytes = Encoding.GetBytes(data);
            Write(bytes);//测试用
        }

        /// <summary>
        /// 发送二进制数组
        /// </summary>
        /// <param name="bytes">二进制数组</param>
        /// <returns>WaitHandle，可以用来检测当写操作完成</returns>
        public void Write(byte[] bytes)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            //开始异步发送操作
            networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, null);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
                isConnected = false;
            }
        }

        /// <summary>
        /// 发送操作的回调
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void WriteCallback(IAsyncResult result)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.EndWrite(result);
        }

        /// <summary>
        /// 连接操作的回调
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                tcpClient.EndConnect(result);
            }
            catch
            {
                //用一个线程安全的方式增加失败的连接数
                //Interlocked.Increment(ref failedConnectionCount);
                //if (failedConnectionCount >= addresses.Length)
                //{
                //没有连接到所有的IP地址
                //整体连接失败.
                isConnected = false;
                return;
                //}
            }
            //设置连接标志
            isConnected = true;
            //连接成功
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            //开始异步读取
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        /// <summary>
        /// 读操作的回调
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ReadCallback(IAsyncResult result)
        {
            int read;
            NetworkStream networkStream;
            try
            {
                networkStream = tcpClient.GetStream();
                read = networkStream.EndRead(result);
            }
            catch
            {
                //读取时发生错误
                return;
            }

            if (read == 0)
            {
                //连接已关闭
                isConnected = false;
                return;
            }

            byte[] buffer = result.AsyncState as byte[];
            string data = this.Encoding.GetString(buffer, 0, read);
            //这里获取数据对象
            if (receiveProcess != null)
            {
                receiveProcess(data);
            }
            //继续读取数据
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        /// <summary>
        /// 获取主机地址操作的回调
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void GetHostAddressesCallback(IAsyncResult result)
        {
            addresses = Dns.EndGetHostAddresses(result);
            //信号地址设置
            ((AutoResetEvent)addressesSet).Set();
        }
    }

}
