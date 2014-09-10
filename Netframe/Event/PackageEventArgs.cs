using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Model;
using System.Net;

namespace Netframe.Event
{
    /// <summary>
    /// UdpPacketMsg 对应所以事件的参数
    /// </summary>
    public class PackageEventArgs : EventArgs
    {
        public PacketNetWorkMsg PacketMsg;
        public  IPEndPoint RemoteIP { get; set; }
        public PacketNetWorkMsg[] Packs;
        public byte[] Data { get; set; }
        /// <summary>
        /// 是否已经处理
        /// </summary>
        public bool IsHandled { get; set; }

        public PackageEventArgs()
        {
            IsHandled = false;
        }
        public PackageEventArgs(PacketNetWorkMsg udpp)
        {
            IsHandled = false;
            this.PacketMsg = udpp;
        }
        public PackageEventArgs(PacketNetWorkMsg[] Packs)
        {
            IsHandled = false;
            this.Packs = Packs;
        }
        public PackageEventArgs(byte[] buff, IPEndPoint remotetip)
        {
            this.Data = buff;
            this.RemoteIP = remotetip;
            IsHandled = false;
        }

        public PackageEventArgs(PacketNetWorkMsg udpp, IPEndPoint remotetip)
        {
            // TODO: Complete member initialization
            this.PacketMsg = udpp;
            this.RemoteIP = remotetip;
        }
    }
}
