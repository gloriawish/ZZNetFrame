using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Netframe.Model;
using System.Net;
using System.Threading;
using Netframe.Tool;
using Netframe.Event;

namespace Netframe.Core
{
    /// <summary>
    /// 基本通信类  UDP，能够进行基本数据发送,UdpPacketMsg的发送，数据收到时触发事件
    /// </summary>
    public class UDPThread
    {
        #region 私有变量

        /// <summary>
        /// 配置信息
        /// </summary>
        Config _config;

        /// <summary>
        /// UDP客户端
        /// </summary>
        UdpClient client;

        /// <summary>
        /// 用于轮询是否发送成功的记录
        /// </summary>
        List<PacketNetWorkMsg> SendList;

        #endregion

        #region 属性

        /// <summary>
        /// 是否已经初始化了
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否建立连接
        /// </summary>
        public bool IsConnect { get; private set; }
        /// <summary>
        /// 检查发送队列间隔
        /// </summary>
        public int CheckQueueTimeInterval { get; set; }

        /// <summary>
        /// 没有收到确认包时，最大重新发送的数目，超过此数目会丢弃并触发PackageSendFailture 事件。
        /// </summary>
        public int MaxResendTimes { get; set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造一个新的消息对象，并绑定到指定的端口和IP上。
        /// </summary>
        /// <param name="ip">绑定的IP</param>
        /// <param name="port">绑定的端口</param>
        public UDPThread(int port)
        {
            IsInitialized = false;
            IPAddress LocalIPAddress = null;
            //获得本机当前的ip
            try
            {
                IPAddress[] address = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress addr in address)
                {
                    if (addr.AddressFamily.ToString().Equals("InterNetwork"))
                    {
                        LocalIPAddress = addr;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                OnLocalIpError(new EventArgs());
                //获取本机ip异常
                return;
            }
            try
            {
                client = new UdpClient(new IPEndPoint(LocalIPAddress, port));
                IsConnect = false;
            }
            catch (Exception)
            {
                OnNetworkError(new EventArgs());
                return;
            }
            SendList = new List<PacketNetWorkMsg>();
            client.EnableBroadcast = true;

            CheckQueueTimeInterval = 2000;
            MaxResendTimes = 5;
            new Thread(new ThreadStart(CheckUnConfirmedQueue)) { IsBackground = true }.Start();


            IsInitialized = true;

            //开始监听
            client.BeginReceive(ReceiveDataAsync, null);
            //ReceiveData();
        }

        public UDPThread(Config config)
        {
            IsInitialized = false;
            try
			{
                client = new UdpClient(new IPEndPoint(config.BindedIP, config.Port));
			}
            catch (Exception)
            {
                OnNetworkError(new EventArgs());
                return;
            }
            SendList = new List<PacketNetWorkMsg>();
            client.EnableBroadcast = true;
            this._config = config;
            CheckQueueTimeInterval = 2000;
            MaxResendTimes = 5;
            new Thread(new ThreadStart(CheckUnConfirmedQueue)) { IsBackground = true }.Start();


            IsInitialized = true;

            //开始监听
            client.BeginReceive(ReceiveDataAsync, null);
        }
        /// <summary>
        /// 构造函数与远程主机连接
        /// </summary>
        /// <param name="ipaddress">绑定ip</param>
        /// <param name="port">端口</param>
        public UDPThread(string ip, int port)
        {
            IsInitialized = false;
            IPAddress ipaddress = IPAddress.Parse(ip);//构造远程连接的参数
            try
            {
                client = new UdpClient();
                client.Connect(new IPEndPoint(ipaddress, port));//与远程服务器建立连接ps:只是形式上,udp本身无连接的
                IsConnect = true;
            }
            catch (Exception)
            {
                OnNetworkError(new EventArgs());
                return;
            }
            SendList = new List<PacketNetWorkMsg>();
            client.EnableBroadcast = true;

            CheckQueueTimeInterval = 2000;
            MaxResendTimes = 5;
            new Thread(new ThreadStart(CheckUnConfirmedQueue)) { IsBackground = true }.Start();


            IsInitialized = true;

            //开始监听
            client.BeginReceive(ReceiveDataAsync, null);
            //ReceiveData();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 接收数据的方法
        /// </summary>
        /// <param name="ar"></param>
        void ReceiveDataAsync(IAsyncResult ar)
        {
            IPEndPoint ipend = null;
            byte[] buffer = null;
            try
            {
                buffer = client.EndReceive(ar, ref ipend);
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                if (IsInitialized && client != null) 
                    client.BeginReceive(ReceiveDataAsync, null);
            }

            if (buffer == null || buffer.Length == 0) return;
            //触发已收到事件
            OnPackageReceived(new PackageEventArgs() { RemoteIP = ipend, Data = buffer });

        }
        /// <summary>
        /// 同步数据接收方法
        /// </summary>
        private void ReceiveData()
        {
            while (true)
            {
                IPEndPoint retip = null;
                byte[] buffer = null;
                try
                {
                    buffer = client.Receive(ref retip);//接收数据,当Client端连接主机的时候，retip就变成Cilent端的IP了
                }
                catch (Exception)
                {
                    //异常处理操作
                    return;
                }
                if (buffer == null || buffer.Length == 0) return;
                PackageEventArgs arg = new PackageEventArgs(buffer, retip);
                OnPackageReceived(arg);//数据包收到触发事件
            }
        }

        /// <summary>
        /// 异步接受数据
        /// </summary>
        private void AsyncReceiveData()
        {
            try
            {
                client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
            catch (SocketException ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 接收数据的回调函数
        /// </summary>
        /// <param name="param"></param>
        private void ReceiveCallback(IAsyncResult param)
        {
            if (param.IsCompleted)
            {
                IPEndPoint retip = null;
                byte[] buffer = null;
                try
                {
                    buffer = client.EndReceive(param, ref retip);//接收数据,当Client端连接主机的时候，test就变成Cilent端的IP了
                }
                catch (Exception ex)
                {
                    //异常处理操作
                }
                finally
                {
                    AsyncReceiveData();
                }
                if (buffer == null || buffer.Length == 0) return;
                OnPackageReceived(new PackageEventArgs() { RemoteIP = retip, Data = buffer });
            }
        }

        #endregion

        #region 公共函数

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void Close()
        {
            if (IsInitialized)
            {
                IsInitialized = false;
                if (IsInitialized) 
                    client.Close();
                IsConnect = false;
                client = null;
            }
        }

        
        /// <summary>
        /// 发送数据，不进行检查
        /// </summary>
        /// <param name="address">远程主机地址</param>
        /// <param name="port">远程主机端口</param>
        /// <param name="data">数据流</param>
        /// <param name="packageNo">数据包编号</param>
        /// <param name="packageIndex">分包索引</param>
        private void Send(IPAddress address, int port, byte[] data, long packageNo, int packageIndex)
        {
            Send(false, new IPEndPoint(address, port), data, packageNo, packageIndex);
        }
        /// <summary>
        /// 发送数据，并判断是否对数据作回应检查。将会在每隔 <see cref="CheckQueueTimeInterval"/> 的间隔后重新发送，直到收到对方的回应。
        /// 注意：网络层不会解析回应，请调用 <see cref="PopSendItemFromList"/> 方法来告知已收到数据包。
        /// </summary>
        /// <param name="receiveConfirm">消息是否会回发确认包</param>
        /// <param name="address">远程主机地址</param>
        /// <param name="port">远程主机端口</param>
        /// <param name="data">数据流</param>
        /// <param name="packageNo">数据包编号</param>
        /// <param name="packageIndex">分包索引</param>
        private void Send(bool receiveConfirm, IPAddress address, int port, byte[] data, long packageNo, int packageIndex)
        {
            Send(receiveConfirm, new IPEndPoint(address, port), data, packageNo, packageIndex);
        }

        /// <summary>
        /// 发送数据，并对数据作回应检查。当 <see cref="receiveConfirm"/> 为 true 时，将会在每隔 <see cref="CheckQueueTimeInterval"></see> 的间隔后重新发送，直到收到对方的回应。
        /// 注意：网络层不会解析回应，请调用 <see cref="PopSendItemFromList"></see> 方法来告知已收到数据包。
        /// </summary>
        /// <param name="receiveConfirm">消息是否会回发确认包</param>
        /// <param name="address">远程主机地址</param>
        /// <param name="data">数据流</param>
        /// <param name="packageNo">数据包编号</param>
        /// <param name="packageIndex">分包索引</param>
        private void Send(bool receiveConfirm, IPEndPoint address, byte[] data, long packageNo, int packageIndex)
        {
            if (IsInitialized)
            {
                client.Send(data, data.Length, address);
                if (receiveConfirm)
                    PushSendItemToList(new PacketNetWorkMsg() { Data = data, RemoteIP = address, SendTimes = 0, PackageIndex = packageIndex, PackageNo = packageNo });
            }
        }
         

        /// <summary>
        /// 同步发送分包数据
        /// </summary>
        /// <param name="message"></param>
        public void SendMsg(Msg message)
        {
            if (IsInitialized)
            {
                ICollection<PacketNetWorkMsg> udpPackets = MessagePacker.BuildNetworkMessage(message);
                foreach (PacketNetWorkMsg packedMessage in udpPackets)
                {
                    //使用同步发送
                    SendPacket(packedMessage);
                }
            }
        }
        /// <summary>
        /// 将已经打包的消息发送出去
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(PacketNetWorkMsg packet)
        {
            if (IsInitialized)
            {
                //使用同步的方法发送数据
                if (!IsConnect)
                    client.Send(packet.Data, packet.Data.Length, packet.RemoteIP);
                else
                    client.Send(packet.Data, packet.Data.Length);
                if (packet.IsRequireReceiveCheck)
                    PushSendItemToList(packet);
            }
        }
        /// <summary>
        /// 异步分包发送数组的方法
        /// </summary>
        /// <param name="message"></param>
        public void AsyncSendMsg(Msg message)
        {
            if (IsInitialized)
            {
                ICollection<PacketNetWorkMsg> udpPackets = MessagePacker.BuildNetworkMessage(message);
                foreach (PacketNetWorkMsg packedMessage in udpPackets)
                {
                    //使用异步的方法发送数据
                    AsyncSendPacket(packedMessage);
                }
            }
        
        }
        /// <summary>
        /// 发送完成后的回调方法
        /// </summary>
        /// <param name="param"></param>
        private void SendCallback(IAsyncResult param)
        {
            if (param.IsCompleted)
            {
                try
                {
                    client.EndSend(param);//这句话必须得写，BeginSend（）和EndSend（）是成对出现的 
                }
                catch (Exception)
                {
                    PackageEventArgs e = new PackageEventArgs();
                    OnPackageSendFailure(e);//触发发送失败事件
                }
            }

        }
        /// <summary>
        /// 异步将将已经打包的消息发送出去,不进行发送检查
        /// </summary>
        /// <param name="packet"></param>
        public void AsyncSendPacket(PacketNetWorkMsg packet)
        {
            //使用异步的方法发送数据
            if (IsInitialized)
            {
                if (!IsConnect)
                    this.client.BeginSend(packet.Data, packet.Data.Length, packet.RemoteIP, new AsyncCallback(SendCallback), null);
                else
                    this.client.BeginSend(packet.Data, packet.Data.Length, new AsyncCallback(SendCallback), null);
                if (packet.IsRequireReceiveCheck)
                    PushSendItemToList(packet);//将该消息压入列表
            }
        }

        #endregion
        System.Threading.SendOrPostCallback cucqCallpack;
        System.Threading.SendOrPostCallback resendCallback;
        /// <summary>
        /// 自由线程，检测未发送的数据并发出
        /// </summary>
        void CheckUnConfirmedQueue()
        {
            //异步调用委托
            if (cucqCallpack == null) cucqCallpack = (s) => OnPackageSendFailure(s as PackageEventArgs);
            if (resendCallback == null) resendCallback = (s) => OnPackageResend(s as PackageEventArgs);
            do
            {
                if (SendList.Count > 0)
                {
                    PacketNetWorkMsg[] array = null;
                    lock (SendList)
                    {
                        array = SendList.ToArray();
                    }
                    //挨个重新发送并计数
                    Array.ForEach(array, s =>
                    {
                        s.SendTimes++;
                        if (s.SendTimes >= MaxResendTimes)
                        {
                            //发送失败啊
                            PackageEventArgs e = new PackageEventArgs();
                            if (SeiClient.NeedPostMessage)
                            {
                                SeiClient.SendSynchronizeMessage(cucqCallpack, e);
                            }
                            else
                            {
                                OnPackageSendFailure(e);//触发发送失败事件
                            }
                            SendList.Remove(s);
                        }
                        else
                        {
                            //重新发送
                            AsyncSendPacket(s);
                            PackageEventArgs e = new PackageEventArgs() { PacketMsg = s };
                            if (SeiClient.NeedPostMessage)
                            {
                                SeiClient.SendASynchronizeMessage(resendCallback, e);
                            }
                            else
                            {
                                OnPackageResend(e);//触发重新发送事件
                            }
                        }
                    });
                }
                Thread.Sleep(CheckQueueTimeInterval);
            } while (IsInitialized);
        }


        static object lockObj = new object();
        /// <summary>
        /// 将数据信息压入列表
        /// </summary>
        /// <param name="item"></param>
        public void PushSendItemToList(PacketNetWorkMsg item)
        {
            SendList.Add(item);
        }

        /// <summary>
        /// 将数据包从列表中移除
        /// 网络层不会解析
        /// </summary>
        /// <param name="packageNo">数据包编号</param>
        /// <param name="packageIndex">数据包分包索引</param>
        public void PopSendItemFromList(long packageNo, int packageIndex)
        {
            lock (lockObj)
            {
                Array.ForEach(SendList.Where(s => s.PackageNo == packageNo && s.PackageIndex == packageIndex).ToArray(), s => SendList.Remove(s));
            }
        }

        #region 事件

        /// <summary>
        /// 网络出现异常,无法获取本地ip地址
        /// </summary>
        public event EventHandler IPError;

        protected void OnLocalIpError(EventArgs e)
        {
            if (IPError != null) IPError(this, e);
        }

        /// <summary>
        /// 网络出现异常（如端口无法绑定等，此时无法继续工作）
        /// </summary>
        public event EventHandler NetworkError;

        protected void OnNetworkError(EventArgs e)
        {
            if (NetworkError != null) NetworkError(this, e);
        }

        /// <summary>
        /// 当数据包收到时触发
        /// </summary>
        public event EventHandler<PackageEventArgs> PackageReceived;

        /// <summary>
        /// 当数据包收到事件触发时，被调用
        /// </summary>
        /// <param name="e">包含事件的参数</param>
        protected virtual void OnPackageReceived(PackageEventArgs e)
        {
            if (PackageReceived != null) PackageReceived(this, e);
        }
        /// <summary>
        /// 数据包发送失败
        /// </summary>
        public event EventHandler<PackageEventArgs> PackageSendFailure;

        /// <summary>
        /// 当数据发送失败时调用
        /// </summary>
        /// <param name="e">包含事件的参数</param>
        protected virtual void OnPackageSendFailure(PackageEventArgs e)
        {
            if (PackageSendFailure != null) PackageSendFailure(this, e);
        }

        /// <summary>
        /// 数据包未接收到确认，重新发送
        /// </summary>
        public event EventHandler<PackageEventArgs> PackageResend;


        /// <summary>
        /// 触发重新发送事件
        /// </summary>
        /// <param name="e">包含事件的参数</param>
        protected virtual void OnPackageResend(PackageEventArgs e)
        {
            if (PackageResend != null) PackageResend(this, e);
        }


        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 关闭客户端并释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion


    }
}
