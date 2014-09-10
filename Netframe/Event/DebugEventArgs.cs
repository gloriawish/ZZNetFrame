using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Model;
using System.Net;

namespace Netframe.Event
{
    /// <summary>
    /// Debug对应所以事件的参数
    /// </summary>
    public class DebugEventArgs : EventArgs
    {
        public string DebugMsg;
        /// <summary>
        /// 是否已经处理
        /// </summary>
        public bool IsHandled { get; set; }

        public DebugEventArgs()
        {
            IsHandled = false;
        }
        public DebugEventArgs(string msg)
        {
            IsHandled = false;
            this.DebugMsg = msg;
        }

    }
}
