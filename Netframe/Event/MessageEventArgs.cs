using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Model;

namespace Netframe.Event
{
    /// <summary>
    /// 消息收到事件参数
    /// </summary>
    public class MessageEventArgs:EventArgs
    {
        /// <summary>
        /// 消息实体
        /// </summary>
        public Msg msg;

        /// <summary>
        /// 发送到的主机
        /// </summary>
        public Host host { get; set; }

        /// <summary>
        /// 是否已经处理过了
        /// </summary>
        public bool IsHandled { get; set; }
        public MessageEventArgs(Msg msg)
        {
            this.msg = msg;
            IsHandled = false;
        }
        public MessageEventArgs(Msg msg, Host host)
        {
            this.msg = msg;
            this.host = host;
            IsHandled = false;
        }
    }
}
