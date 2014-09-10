using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Netframe.Model
{
    /*
     * udp分包发送的数据
     * **/
    public class PacketNetWorkMsg : IComparable<PacketNetWorkMsg>
    {
        /// <summary>
		/// 封包版本
		/// </summary>
		public int Version { get; set; }

		/// <summary>
		/// 要发送的数据包
		/// </summary>
		public byte[] Data { get; set; }

        /// <summary>
        /// 数据包所含数据长度
        /// </summary>
        public int DataLength { get; set; }

        /// <summary>
        /// 分包后最后一个剩余长度
        /// </summary>
        public int Remainder { get; set; }
		/// <summary>
		/// 远程地址
		/// </summary>
		public IPEndPoint RemoteIP { get; set; }

		/// <summary>
		/// 发送次数
		/// </summary>
		public int SendTimes { get; set; }

		/// <summary>
		/// 包编号
		/// </summary>
		public long PackageNo { get; set; }

		/// <summary>
		/// 分包索引
		/// </summary>
		public int PackageIndex { get; set; }

		/// <summary>
		/// 分包总数
		/// </summary>
		public int PackageCount { get; set; }

		/// <summary>
		/// 获得或设置是否需要返回已收到标志
		/// </summary>
        public bool IsRequireReceiveCheck { get; set; }

        public PacketNetWorkMsg()
		{
			Version = 1;
            CreationTime = DateTime.Now;
		}
        public PacketNetWorkMsg(long packageNo, int Count, int index, byte[] data, int dataLength, int remainder, IPEndPoint desip, bool IsRequireReceive)
        {
            this.PackageNo = packageNo;
            this.PackageCount = Count;
            this.PackageIndex = index;
            this.Data = data;
            this.DataLength = dataLength;
            this.Remainder = remainder;
            this.IsRequireReceiveCheck = IsRequireReceive;//默认都需要确认包
            this.RemoteIP = desip;
        }
		#region IComparable<PackedNetworkMessage> 成员

        public int CompareTo(PacketNetWorkMsg other)
		{
			return PackageIndex < other.PackageIndex ? -1 : 1;
		}

		#endregion

        /// <summary>
        /// 获得生成数据包的时间
        /// </summary>
        public DateTime CreationTime { get; private set; }
    }
}
