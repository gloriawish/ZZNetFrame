using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Model;
using System.Net;
using Netframe.Core;
using Netframe.Tool;
using Netframe.Event;
using System.Threading;

namespace Netframe
{
    /// <summary>
    /// 核心客户端对象
    /// </summary>
    public class SeiClient
    {
        bool _isInitialized = false;

        #region 属性
        /// <summary>
        /// 返回当前的配置对象
        /// </summary>
        public Config Config { get; private set; }

        /// <summary>
        /// UDP文本信息网络层对象
        /// </summary>
        public UDPThread MessageClient { get { return Commander.Client; } }

        /// <summary>
        /// 文本信息翻译层对象
        /// </summary>
        public MsgTranslator MessageProxy { get { return Commander.MessageProxy; } }


        /// <summary>
        /// 代表自己的主机信息
        /// </summary>
        public Host Host { get; private set; }

        /// <summary>
        /// 命令解释执行对象
        /// </summary>
        public CommandExecutor Commander { get; private set; }

        /// <summary>
        /// 返回是否初始化成功
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return this._isInitialized;
            }
        }

        SynchronizationContext _context;
        /// <summary>
        /// 获得创建客户端时所使用的线程上下文
        /// </summary>
        public SynchronizationContext ThreadContext
        {
            get { return _context; }
        }
        #endregion

        #region 构造函数
        /// <summary>
		/// 创建一个新的客户端
		/// </summary>
		/// <param name="port">使用的端口</param>
		/// <param name="username">昵称</param>
		/// <param name="userGroup">用户组</param>
		/// <param name="ip">要绑定到的IP地址</param>
		/// <remarks></remarks>
        public SeiClient(int port, string username, string userGroup, IPAddress ip)
		{
            if (Config == null) Config = new Config
											{
												AbsenceMessage = "有事暂时不在，稍后联系你",
												IsInAbsenceMode = false,
												IgnoreNoAddListFlag = false,
												AutoReply = false,
												AutoReplyMessage = "暂时不在，稍后联系你",
												AutoReplyWhenAbsence = true,
												EnableHostNotifyBroadcast = false,
											};

            Config.Port = port > 0 && port <= 65535 ? port : 2425;
            Config.NickName = !string.IsNullOrEmpty(username) ? username : "Sei Client";
            Config.GroupName = !string.IsNullOrEmpty(userGroup) ? userGroup : "By zz";
            Config.BindedIP = ip;

            Initialize(Config);
		}
        /// <summary>
		/// 使用默认参数构建一个新的客户端
		/// </summary>
		/// <remarks></remarks>
		public SeiClient()
			: this(2425, null, null, IPAddress.Any)
		{
		}
        /// <summary>
		/// 指定创建的端口
		/// </summary>
		/// <param name="port">端口</param>
		/// <remarks></remarks>
		public SeiClient(int port)
			: this(port, null, null, IPAddress.Any)
		{
		}
        /// <summary>
		/// 指定创建的端口和IP的对象
		/// </summary>
		/// <param name="port">端口</param>
		/// <param name="ip">要绑定到的IP地址</param>
		/// <remarks></remarks>
		public SeiClient(int port, IPAddress ip)
			: this(port, null, null, ip)
		{
		}
        /// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="cfg">要使用的配置</param>
		public SeiClient(Config cfg)
		{
			Initialize(cfg);
            InitializeEvents();
		}
        /// <summary>
        /// 使用配置的XML文件创建对象
        /// </summary>
        /// <param name="configXml">配置的XML内容</param>
        /// <returns></returns>
        public static SeiClient Create(string configXml)
        {
            Config cfg = Config.CreateConfigFromString(configXml);

            return new SeiClient(cfg);
        }
        #endregion
        #region 公共函数
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="cfg">初始化使用的配置</param>
        private void Initialize(Config cfg)
        {
            if (_isInitialized) throw new InvalidOperationException("already initialized.");

            System.Diagnostics.Debug.WriteLine("IPMClient 开始初始化...端口：" + cfg.Port.ToString());

            this.Config = cfg;

            _context = SynchronizationContext.Current;

            if (cfg.BindedIP == null) 
                cfg.BindedIP = IPAddress.Any;


            //开始构造对象
            Commander = new CommandExecutor(this.Config);
            Commander.Init();


            this._isInitialized = Commander.IsInitialized;
        }
        #endregion

        #region 辅助函数

        /// <summary>
        /// 生成默认的设置
        /// </summary>
        /// <returns></returns>
        public static Config GetDefaultConfig()
        {
            return new Config
            {
                Port = 2425,
                GroupName = Environment.MachineName,
                NickName = Environment.UserName,
                IgnoreNoAddListFlag = false,
                EnableHostNotifyBroadcast = false,
                HostName = Environment.MachineName,
                AutoReplyWhenAbsence = true,
                AutoDetectVersion = true,
                BanedHost = new List<string>(),
                KeepedHostList = new List<string>(),
                BindedIP = IPAddress.Any,
                VersionInfo = String.Format("SeiClient.Net {0}，BY zz", System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion),
                AbsenceSuffix = " [离开]",
            };
        }

        #endregion

        #region 事件定义

        /// <summary>
        /// 初始化事件
        /// </summary>
        void InitializeEvents()
        {
            if (!this.IsInitialized) return;

            //挂载一些内部事件
            this.Commander.TextMessageReceived += Commander_TextMessageReceived;
        }

        void Commander_TextMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.IsHandled) 
                return;
        }
        #endregion


        #region 线程同步

        /// <summary>
        /// 线程同步上下文
        /// </summary>
        internal static SynchronizationContext SynchronizationContext;

        /// <summary>
        /// 返回是否需要提交线程同步信息
        /// </summary>
        internal static bool NeedPostMessage { get { return SynchronizationContext != null; } }

        /// <summary>
        /// 提交同步消息
        /// </summary>
        /// <param name="call">调用的委托</param>
        /// <param name="arg">参数</param>
        internal static void SendSynchronizeMessage(SendOrPostCallback call, object arg)
        {
            if (!NeedPostMessage) call(arg);
            else SynchronizationContext.Send(call, arg);
        }

        /// <summary>
        /// 提交异步消息
        /// </summary>
        /// <param name="call">调用的委托</param>
        /// <param name="arg">参数</param>
        internal static void SendASynchronizeMessage(SendOrPostCallback call, object arg)
        {
            if (!NeedPostMessage) call(arg);
            else SynchronizationContext.Post(call, arg);
        }


        #endregion
    }
}
