using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Model;
using Netframe.Event;
using System.Threading;

namespace Netframe.Core
{
    /// <summary>
    /// 用来解析从tran层传来的消息
    /// </summary>
    public class CommandExecutor 
    {
        #region 属性
        /// <summary>
        /// UDP通信类
        /// </summary>
        public UDPThread Client { get; set; }

        /// <summary>
        /// 消息发送和代理类
        /// </summary>
        public MsgTranslator MessageProxy { get; set; }


        private Config _config;
        /// <summary>
        /// 配置类
        /// </summary>
        public Config Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
            }
        }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        internal CommandExecutor(Config config)
        {
            Config = config;
        }
        public void Init()
        {
            Client = new UDPThread(_config);
            MessageProxy = new MsgTranslator(Client, this._config);
            //与接收消息进行挂钩
            MessageProxy.MessageReceived += MessageProxy_MessageReceived;
        }

        /// <summary>
        /// 获得底层是否成功初始化的状态
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return Client.IsInitialized;
            }
        }

        #region 消息处理
        //处理接收到消息事件
        void MessageProxy_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.msg == null || e.IsHandled) return;
            MessageEventArgs me = new MessageEventArgs(e.msg);
            if (me.IsHandled) return;
            //分析请求
            switch (e.msg.Command)
            {
                case Commands.Entry:
                    ProcessCommand_Entry(e.msg); 
                    break;
                default: break;
            }
        }
        
        private void ProcessCommand_Entry(Msg m)
        {
            if (m == null)
                return;
            OnTextMessageReceived(new MessageEventArgs(m));
        }
        #endregion

        /// <summary>
        /// 收到文本消息事件
        /// </summary>
        public event EventHandler<MessageEventArgs> TextMessageReceived;
        SendOrPostCallback textMessageReceivedCall;
        /// <summary>
        /// 触发收到文本消息事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected virtual void OnTextMessageReceived(MessageEventArgs e)
        {
            if (TextMessageReceived == null) 
                return;
            if (SeiClient.NeedPostMessage)
            {
                if (textMessageReceivedCall == null) textMessageReceivedCall = s => TextMessageReceived(this, s as MessageEventArgs);
                SeiClient.SendSynchronizeMessage(textMessageReceivedCall, e);
            }
            else
            {
                TextMessageReceived(this, e);
            }
        }
    }
}
