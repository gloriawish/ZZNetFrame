using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Netframe.Model
{
    public class Msg
    {
        public static int Seed = 0;
        /// <summary>
        /// 是否已经被处理.在挂钩过程中,如果为true,则底层代码不会再对信息进行处理
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 获得或设置当前的消息编号
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public long PackageNo { get; set; }

        /// <summary>
        /// 获得或设置当前的消息所属的主机名
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 获得或设置当前的消息所属的用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 获得或设置当前的命令代码
        /// </summary>
        //命令的名称
        public Commands Command { get; set; }

        /// <summary>
        /// 获得或设置当前的消息的类型 文本消息，或者二进制消息
        /// </summary>
        public Consts Type { get; set; }

        /// <summary>
        /// 获得或设置当前的命令消息文本
        /// </summary>
        public string NormalMsg { get; set; }

        /// <summary>
        /// 消息文本字节
        /// </summary>
        public byte[] NormalMsgBytes { get; set; }

        /// <summary>
        /// 扩展消息文本字节
        /// </summary>
        public byte[] ExtendMessageBytes { get; set; }

        /// <summary>
        /// 获得或设置当前命令的扩展文本
        /// </summary>
        public string ExtendMessage { get; set; }

        /// <summary>
        /// 远程地址
        /// </summary>
        public IPEndPoint RemoteAddr { get; set; }

        /// <summary>
        /// 主机地址
        /// </summary>
        public IPEndPoint HostAddr { get; set; }
        /// <summary>
        /// 获得或设置是否需要返回已收到标志
        /// </summary>
        public bool IsRequireReceive { get; set; }

        private Host _host;
        /// <summary>
        /// 关联的主机
        /// </summary>
        public Host Host
        {
            get
            {
                return _host;
            }
            set
            {
                _host = value;
                if (value != null) HostAddr = value.HostSub.Ipv4Address;
            }
        }
        public Msg(IPEndPoint Addr)
		{
			RemoteAddr = Addr;
			Handled = false;
            Type = Consts.MESSAGE_TEXT;
		}
		public Msg(IPEndPoint remote, string hostName, string userName,Commands command, string message, string extendMessage)
		{
            RemoteAddr = remote;
			Handled = false;
			HostName = hostName;
			UserName = userName;
			Command = command;
			NormalMsg = message;
			ExtendMessage = extendMessage;
            Type = Consts.MESSAGE_TEXT;
		}


		/// <summary>
		/// 直接创建一个新的Message对象
		/// </summary>
		/// <param name="host">主机对象</param>
		/// <param name="addr">远程地址</param>
		/// <param name="hostName">主机名</param>
		/// <param name="userName">用户名</param>
		/// <param name="command">命令</param>
		/// <param name="options">选项</param>
		/// <param name="message">信息</param>
		/// <param name="extendMessage">扩展信息</param>
		/// <returns></returns>
        public static Msg Create(Host host, IPEndPoint remote, string hostName, string userName, Commands command, string message, string extendMessage)
		{
            return new Msg(remote, hostName, userName, command, message, extendMessage);
		}
        public static Msg Create(IPEndPoint remote, string hostName, string userName, Commands command, string message, string extendMessage)
        {
            return new Msg(remote, hostName, userName, command, message, extendMessage);
        }
        public static long GetRandomNumber()
        {
            Random Rd = new Random(unchecked((int)DateTime.Now.Millisecond));
            return (long)Rd.Next()+(Seed++);
        }
    }
}
