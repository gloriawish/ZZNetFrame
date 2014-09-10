using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Netframe.Model
{
    /// <summary>
    /// 使用的客户端通信配置
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class Config
    {

        #region 属性
        /// <summary>
        /// 已经被屏蔽的主机IP列表
        /// </summary>
        public List<string> BanedHost { get; set; }

        private List<IPAddress> _keepedHostList_Addr;

        /// <summary>
        /// DialUp的主机IP列表
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        internal List<IPAddress> KeepedHostList_Addr
        {
            get
            {
                if (_keepedHostList_Addr == null)
                {
                    if (_keepedHostList_Addr == null) _keepedHostList_Addr = new List<IPAddress>();
                    else _keepedHostList_Addr.Clear();

                    Array.ForEach(KeepedHostList.ToArray(), (s) =>
                    {
                        _keepedHostList_Addr.Add(IPAddress.Parse(s));
                    });
                }
                return _keepedHostList_Addr;
            }
            set
            {
                _keepedHostList_Addr = value;
            }
        }
        /// <summary>
        /// 将主机从拨号列表中移除
        /// </summary>
        /// <param name="address"></param>
        public void RemoveHostFromDialList(string address)
        {
            IPAddress ip = IPAddress.Parse(address);

            if (!KeepedHostList.Contains(address)) return;

            KeepedHostList.Remove(address);
            KeepedHostList_Addr.Remove(ip);
        }

        private List<string> _keepedHostList;
        /// <summary>
        /// DialUp的主机IP列表
        /// </summary>
        public List<string> KeepedHostList
        {
            get
            {
                if (_keepedHostList == null) _keepedHostList = new List<string>();
                return _keepedHostList;
            }
            set
            {
                _keepedHostList = value;
            }
        }
        /// <summary>
        /// 离开时自动回复
        /// </summary>
        public bool AutoReplyWhenAbsence { get; set; }

        /// <summary>
        /// 自动回复
        /// </summary>
        public bool AutoReply { get; set; }

        /// <summary>
        /// 自动回复消息
        /// </summary>
        public string AutoReplyMessage { get; set; }

        private string _hostName;
        /// <summary>
        /// 主机名
        /// </summary>
        public string HostName
        {
            get
            {
                if (string.IsNullOrEmpty(_hostName)) return Environment.MachineName;
                else return _hostName;
            }
            set
            {
                _hostName = value;
            }
        }

        private string _hostUserName;
        /// <summary>
        /// 主机用户名
        /// </summary>
        public string HostUserName
        {
            get
            {
                if (string.IsNullOrEmpty(_hostUserName)) return Environment.UserName;
                else return _hostUserName;
            }
            set
            {
                _hostUserName = value;
            }
        }

        #endregion

        #region 状态信息

        /// <summary>
        /// 是否在离开状态
        /// </summary>
        public bool IsInAbsenceMode { get; set; }

        /// <summary>
        /// 离开状态信息
        /// </summary>
        public string AbsenceMessage { get; set; }

        /// <summary>
        /// 离开信息后缀
        /// </summary>
        public string AbsenceSuffix { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public string VersionInfo { get; set; }

        #endregion

        #region 网络设置
        /// <summary>
        /// 通信使用的端口
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>默认是2425,改变此端口后重新启动通信才可</remarks>
        public int Port { get; set; }

        private string _nickName;
        /// <summary>
        /// 飞鸽的用户名
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string NickName
        {
            get
            {
                if (string.IsNullOrEmpty(_nickName)) return this.HostUserName;
                else return _nickName;
            }
            set
            {
                _nickName = value;
            }
        }

        private string _groupName;
        /// <summary>
        /// 用户组(显示为飞鸽的用户组)
        /// </summary>
        /// <value>用户组</value>
        /// <returns>值</returns>
        /// <remarks>默认是 飞鸽用户</remarks>
        public string GroupName
        {
            get
            {
                if (string.IsNullOrEmpty(_groupName)) return this.HostName;
                else return _groupName;
            }
            set
            {
                _groupName = value;
            }
        }

        /// <summary>
        /// Socket的缓冲区大小,只读
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>由C版拓展而来,保留为 65535</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public int SocketBuffer
        {
            get
            {
                return 65536;
            }
        }
        /// <summary>
        /// UDP通信的缓冲区大小
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>由C版转移而来,保留为16384</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public int UdpBuffer
        {
            get
            {
                return 16384;
            }
        }
        /// <summary>
        /// 绑定的IP
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public IPAddress BindedIP { get; set; }

        /// <summary>
        /// 绑定的IP的字符串表现形式
        /// </summary>
        /// <remarks>这是供序列化使用的，请不要在代码中使用它</remarks>
        public string BindedIPString
        {
            get
            {
                return BindedIP.ToString();
            }
            set
            {
                BindedIP = IPAddress.Parse(value);
            }
        }

        /// <summary>
        /// 限制的内容
        /// </summary>
        public string[] FileShareLimits { get; set; }

        /// <summary>
        /// 忽略不加入列表的标志位
        /// </summary>
        public bool IgnoreNoAddListFlag { get; set; }

        /// <summary>
        /// 是否开启通知非本网段主机上线功能
        /// </summary>
        public bool EnableHostNotifyBroadcast { get; set; }

        /// <summary>
        /// 是否允许网络文件传输
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool EnableFileTransfer { get; set; }

        /// <summary>
        /// 忽略回传的消息
        /// </summary>
        public bool IgnoreLoopBack { get; set; }

        /// <summary>
        /// 自动查询主机版本
        /// </summary>
        public bool AutoDetectVersion { get; set; }

        /// <summary>
        /// TCP连接超时
        /// </summary>
        public int ConnectionTimeout { get; set; }
        #endregion

        #region 静态函数

        /// <summary>
        /// 加入一个主机到黑名单
        /// </summary>
        /// <param name="ip"></param>
        public void BanHost(IPAddress ip)
        {
            if (BanedHost == null) BanedHost = new List<string>();
            if (!BanedHost.Contains(ip.ToString())) BanedHost.Add(ip.ToString());
        }

        /// <summary>
        /// 加入一个主机到黑名单
        /// </summary>
        /// <param name="ip"></param>
        public void BanHost(Host ip)
        {
            BanHost(ip.HostSub.Ipv4Address.Address);
        }

        /// <summary>
        /// 检测一个主机是否在黑名单中
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsHostInBlockList(IPAddress ip)
        {
            return (BanedHost != null && BanedHost.Contains(ip.ToString()));
        }

        /// <summary>
        /// 检测一个主机是否在黑名单中
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsHostInBlockList(Host ip)
        {
            return IsHostInBlockList(ip.HostSub.Ipv4Address.Address);
        }

        /// <summary>
        /// 取消指定主机的屏蔽
        /// </summary>
        /// <param name="ip"></param>
        public void UnBanIP(IPAddress ip)
        {
            if (BanedHost != null && BanedHost.Contains(ip.ToString())) BanedHost.Remove(ip.ToString());
        }

        /// <summary>
        /// 清空屏蔽列表
        /// </summary>
        public void UnBanAllIp()
        {
            if (BanedHost != null) BanedHost.Clear();
        }

        //本次启动后包发送的序号
        static ulong packageIndex = 0;

        /// <summary>
        /// 获得随机的编号
        /// </summary>
        /// <returns></returns>
        public static ulong GetRandomTick()
        {
            return (ulong)((new Random()).Next()) + (packageIndex++);
        }

        #endregion

        #region 辅助函数

        /// <summary>
        /// 将本对象序列化
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.Xml.Serialization.XmlSerializer bf = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            bf.Serialize(ms, this);
            ms.Flush();

            string msg = System.Text.Encoding.Default.GetString(ms.ToArray());
            ms.Close();

            return msg;
        }

        /// <summary>
        /// 创建配置
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static Config CreateConfigFromString(string msg)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] buffer = System.Text.Encoding.Default.GetBytes(msg);
            ms.Write(buffer, 0, buffer.Length);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            System.Xml.Serialization.XmlSerializer bf = new System.Xml.Serialization.XmlSerializer(typeof(Config));

            Config cfg = bf.Deserialize(ms) as Config;
            ms.Close();

            if (cfg.Port <= 0 || cfg.Port > 65535) cfg.Port = 2425;
            if (string.IsNullOrEmpty(cfg.NickName)) cfg.NickName = "IPM Client";
            if (!string.IsNullOrEmpty(cfg.GroupName)) cfg.GroupName = "By 随风飘扬";

            return cfg;
        }

        #endregion
    }
}
