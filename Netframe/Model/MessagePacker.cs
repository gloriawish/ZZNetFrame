using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Netframe.Event;

namespace Netframe.Model
{
    /// <summary>
    /// 消息封包类
    /// </summary>
    public class MessagePacker
    {
        Timer _timer;
        public MessagePacker()
		{
			_timer = new Timer(_ => CheckForOutdateMessage(), null, new TimeSpan(0, 5, 0), new TimeSpan(0, 0, 5, 0));
		}
        /*
		 * 消息包注意：
		 * 1.第一位始终是2(ASCII码50)
		 * 2.第二位到第九位是一个long类型的整数，代表消息编号
		 * 3.第十位到第十三位是一个int类型的整数，代表消息内容总长度
		 * 4.第十四位到第十七位是一个int类型的整数，代表分包的总数
		 * 5.第十八位到第二十一位是一个int类型的整数，代表当前的分包编号
		 * 6.第二十二位表示是否需要返回一个确认标识(1/0)
		 * 7.第二十三到第三十一位是保留的(Reserved)
		 * 8.第三十二字节以后是数据包
		 * */

        /// <summary>
        /// 消息版本号
        /// </summary>
        public static byte VersionHeader { get { return 50; } }
        /// <summary>
        /// 返回当前消息封包的头字节数
        /// </summary>
        public static int PackageHeaderLength { get { return 32; } }

        /// <summary>
        /// 获得消息包的字节流
        /// </summary>
        /// <param name="message">要打包的消息对象</param>
        /// <returns></returns>
        public static PacketNetWorkMsg[] BuildNetworkMessage(Msg message)
        {
            if (message.ExtendMessageBytes != null)
            {
                return BuildNetworkMessage(
                message.RemoteAddr,
                message.PackageNo,
                message.Command,
                message.UserName,
                message.HostName,
                message.Type,
                message.NormalMsgBytes,
                message.ExtendMessageBytes,
                message.IsRequireReceive
                );
            }
            else
            {
                return BuildNetworkMessage(
                message.RemoteAddr,
                message.PackageNo,
                message.Command,
                message.UserName,
                message.HostName,
                message.Type,
                System.Text.Encoding.Unicode.GetBytes(message.NormalMsg),
                System.Text.Encoding.Unicode.GetBytes(message.ExtendMessage),
                message.IsRequireReceive
                );
            }
        }

        /// <summary>
        /// 获得消息包的字节流
        /// </summary>
        /// <param name="remoteIp">远程主机地址</param>
        /// <param name="packageNo">包编号</param>
        /// <param name="command">命令</param>
        /// <param name="options">参数</param>
        /// <param name="userName">用户名</param>
        /// <param name="hostName">主机名</param>
        /// <param name="content">正文消息</param>
        /// <param name="extendContents">扩展消息</param>
        /// <returns></returns>
        public static PacketNetWorkMsg[] BuildNetworkMessage(IPEndPoint remoteIp, long packageNo, Commands command, string userName, string hostName,Consts type ,byte[] content, byte[] extendContents, bool RequireReceiveCheck)
        {

            //每次发送所能容下的数据量
            int maxBytesPerPackage = (int)Consts.MAX_UDP_PACKAGE_LENGTH - PackageHeaderLength;
            //压缩数据流
            var ms = new MemoryStream();
            //var dest = new MemoryStream();
            //var zip = new GZipStream(dest, CompressionMode.Compress);
            var bw = new BinaryWriter(ms, System.Text.Encoding.Unicode);
            //写入头部数据
            bw.Write(packageNo);			//包编号
            bw.Write(userName);				//用户名
            bw.Write(hostName);				//主机名
            bw.Write((long)command);        //命令
            bw.Write((long)type);           //数据类型
            bw.Write(content == null ? 0 : content.Length);//数据长度

            //写入消息数据
            if (content != null) 
                bw.Write(content);
            bw.Write(extendContents == null ? 0 : extendContents.Length);//补充数据长度
            if (extendContents != null) 
                bw.Write(extendContents);
           
            
            ms.Flush();
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] ibuf = ms.ToArray();

            var dest = new System.IO.MemoryStream();
            GZipStream zipStream = new GZipStream(dest, CompressionMode.Compress, true);
            byte[] buff = new byte[1024];
            int offset;
            ms.Seek(0, SeekOrigin.Begin);
            while ((offset = ms.Read(buff, 0, buff.Length)) > 0)
            {
                zipStream.Write(buff, 0, offset);//先把数据用二进制写入内存，然后在把它用zip压缩，获取压缩过后的二进制流dest
            }
            zipStream.Close();
            bw.Close();
            ms.Close();
            dest.Seek(0, SeekOrigin.Begin);
            //打包数据总量
            int dataLength = (int)dest.Length;
            
            int packageCount = (int)Math.Ceiling(dataLength * 1.0 / maxBytesPerPackage);
            PacketNetWorkMsg[] pnma = new PacketNetWorkMsg[packageCount];
            for (int i = 0; i < packageCount; i++)
            {
                int count = i == packageCount - 1 ? dataLength - maxBytesPerPackage * (packageCount - 1) : maxBytesPerPackage;

                byte[] buf = new byte[count + PackageHeaderLength];
                buf[0] = VersionHeader;//版本号 第1位 
                BitConverter.GetBytes(packageNo).CopyTo(buf, 1);//消息编号 第2到9位 long类型的整数
                BitConverter.GetBytes(dataLength).CopyTo(buf, 9);//消息内容长度 第10到13位 int类型的整数
                BitConverter.GetBytes(packageCount).CopyTo(buf, 13);//分包总数 第14位到第17位 int类型的整数
                BitConverter.GetBytes(i).CopyTo(buf, 17);//分包编号 第18位到第21位 int类型的整数
                buf[21] = RequireReceiveCheck ? (byte)1 : (byte)0;//是否回确认包 第22位 
                //第23到第31位是保留的(Reserved)
                dest.Read(buf, 32, buf.Length - 32);//第32字节以后是,具体的数据包

                pnma[i] = new PacketNetWorkMsg()
                {
                    Data = buf,
                    PackageCount = packageCount,
                    PackageIndex = i,
                    PackageNo = packageNo,
                    RemoteIP = remoteIp,
                    SendTimes = 0,
                    Version = 2,
                    IsRequireReceiveCheck = buf[21] == 1
                };
            }

            return pnma;
        }


        /// <summary>
        /// 检测确认是否是这个类型的消息包
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool Test(byte[] buffer)
        {
            return buffer != null && buffer.Length > PackageHeaderLength && buffer[0] == VersionHeader;
        }

        /// <summary>
        /// 缓存接收到的片段
        /// </summary>
        static Dictionary<long, PacketNetWorkMsg[]> packageCache = new Dictionary<long, PacketNetWorkMsg[]>();

        /// <summary>
        /// 分析网络数据包并进行转换为信息对象
        /// </summary>
        /// <param name="packs">接收到的封包对象</param>
        /// <returns></returns>
        /// <remarks>
        /// 对于分包消息，如果收到的只是片段并且尚未接收完全，则不会进行解析
        /// </remarks>
        public static Msg ParseToMessage(params PacketNetWorkMsg[] packs)
        {
            if (packs.Length == 0 || (packs[0].PackageCount > 1 && packs.Length != packs[0].PackageCount))
                return null;


            var ms = DecompressMessagePacks(packs);
            if (ms == null)
            {
                //事件
                OnDecompressFailed(new PackageEventArgs(packs));
                return null;
            }
           //构造读取流
            System.IO.BinaryReader br = new System.IO.BinaryReader(ms, System.Text.Encoding.Unicode);
            //开始读出数据
            Msg m = new Msg(packs[0].RemoteIP);
            m.PackageNo = br.ReadInt64();//包编号

            m.UserName = br.ReadString();//用户名
            m.HostName = br.ReadString();//主机名
            m.Command = (Commands)br.ReadInt64(); //命令
            m.Type = (Consts)br.ReadInt64();//数据类型
            int length = br.ReadInt32(); //数据长度
            m.NormalMsgBytes = new byte[length];
            br.Read(m.NormalMsgBytes, 0, length);//读取内容

            length = br.ReadInt32();    //附加数据长度
            m.ExtendMessageBytes = new byte[length];
            br.Read(m.ExtendMessageBytes, 0, length);//读取附加数据

            if (m.Type == Consts.MESSAGE_TEXT)
            {
                m.NormalMsg = System.Text.Encoding.Unicode.GetString(m.NormalMsgBytes, 0, m.NormalMsgBytes.Length);	//正文
                m.ExtendMessage = System.Text.Encoding.Unicode.GetString(m.ExtendMessageBytes, 0, m.ExtendMessageBytes.Length);	//扩展消息
                m.ExtendMessageBytes = null;
                m.NormalMsgBytes = null;

            }
            return m;
        }
        /// <summary>
        /// 组合所有的网络数据包并执行解压缩
        /// </summary>
        /// <param name="packs"></param>
        /// <returns></returns>
        static MemoryStream DecompressMessagePacks(params PacketNetWorkMsg[] packs)
        {
            try
            {
                //尝试解压缩，先排序
                Array.Sort(packs);
                var msout = new MemoryStream();
                using (var ms = new System.IO.MemoryStream())
                {
                    //合并写入
                    Array.ForEach(packs, s => ms.Write(s.Data, 0, s.Data.Length));
                    ms.Seek(0, SeekOrigin.Begin);

                    //解压缩
                    using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        var buffer = new byte[0x400];
                        var count = 0;
                        while ((count = gz.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            msout.Write(buffer, 0, count);
                        }
                    }
                }
                msout.Seek(0, SeekOrigin.Begin);

                return msout;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 尝试将收到的网络包解析为实体
        /// </summary>
        /// <param name="pack">收到的网络包</param>
        /// <returns></returns>
        /// <remarks>如果收到的包是分片包，且其所有子包尚未接受完全，则会返回空值</remarks>
        public static Msg TryToTranslateMessage(PacketNetWorkMsg pack)
        {
            if (pack == null || pack.PackageIndex > pack.PackageCount - 1) return null;
            else if (pack.PackageCount == 1) return ParseToMessage(pack);
            else
            {
                lock (packageCache)
                {
                    if (packageCache.ContainsKey(pack.PackageNo))
                    {
                        PacketNetWorkMsg[] array = packageCache[pack.PackageNo];
                        array[pack.PackageIndex] = pack;

                        //检测是否完整
                        if (Array.FindIndex(array, s => s == null) == -1)
                        {
                            packageCache.Remove(pack.PackageNo);
                            return ParseToMessage(array);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        PacketNetWorkMsg[] array = new PacketNetWorkMsg[pack.PackageCount];
                        array[pack.PackageIndex] = pack;
                        packageCache.Add(pack.PackageNo, array);
                        return null;
                    }
                }
            }

        }

        /// <summary>
        /// 将网络信息解析为封包
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static PacketNetWorkMsg Parse(byte[] buffer, IPEndPoint clientAddress)
        {
            if (!Test(buffer)) return null;

            PacketNetWorkMsg p = new PacketNetWorkMsg()
            {
                RemoteIP = clientAddress,
                SendTimes = 0
            };
            p.PackageNo = BitConverter.ToInt64(buffer, 1);//包编号
            p.DataLength = (int)BitConverter.ToInt64(buffer, 9); //内容长度
            p.PackageCount = BitConverter.ToInt32(buffer, 13);//分包总数
            p.PackageIndex = BitConverter.ToInt32(buffer, 17);//索引
            p.IsRequireReceiveCheck = buffer[21] == 1;//是否需要回包
            p.Data = new byte[buffer.Length - PackageHeaderLength];
            Array.Copy(buffer, PackageHeaderLength, p.Data, 0, p.Data.Length);

            return p;
        }
        void CheckForOutdateMessage()
        {
            
            lock (packageCache)
            {
                //TODO 这里设置最短的过期时间为5分钟，也就是说五分钟之前的消息会被干掉
                var minTime = DateTime.Now.AddMinutes(5.0);
                var targetList = new List<long>();
                foreach (var pkgid in packageCache.Keys)
                {
                    if (Array.TrueForAll(packageCache[pkgid], s => s == null || s.CreationTime < minTime))
                    {
                        targetList.Add(pkgid);
                    }
                }

                foreach (var pkgid in targetList)
                {
                    packageCache.Remove(pkgid);
                }
            }

        }
        #region 事件
        /// <summary>
        /// 网络层数据包解压缩失败
        /// </summary>
        public static event EventHandler<PackageEventArgs> DecompressFailed;

        /// <summary>
        /// 触发解压缩失败事件
        /// </summary>
        /// <param name="e">事件包含的参数</param>
        protected static void OnDecompressFailed(PackageEventArgs e)
        {
            if (DecompressFailed != null) DecompressFailed(typeof(MessagePacker),e);
        }
        #endregion
    }
}
