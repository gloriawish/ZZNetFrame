using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Netframe.Event;
using Netframe.Model;
using Netframe.Tool;
using System.Threading;

namespace Netframe.Core
{
    /// <summary>
    /// 对底层收到数据的解析
    /// </summary>
    public class MsgTranslator
    {
        #region 属性
        /// <summary>
        /// 用来发送和接收消息的对象
        /// </summary>
        public UDPThread Client { get; set; }

        Config _config;

        //用来检测重复收到的消息包
        Queue<long> ReceivedQueue;

        #endregion

        public MsgTranslator(UDPThread udpClient,Config config)
        {
            this.Client = udpClient;
            this._config = config;
            ReceivedQueue = new Queue<long>();
            Client.PackageReceived += PackageReceived;
        }

        /// <summary>
        /// 发送信息实体
        /// </summary>
        /// <param name="msg"></param>
        public void Send(Msg msg)
        {   
            //消息正在发送事件
            OnMessageSending(new MessageEventArgs(msg));
            Client.AsyncSendMsg(msg);
            //消息已发送事件
            OnMessageSended(new MessageEventArgs(msg));
        }


        static object lockObj = new object();
        /// <summary>
        /// 消息包接收到时的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageReceived(object sender, PackageEventArgs e)
        {
            if (!e.IsHandled)
            {
                e.IsHandled = true;
                Msg m = ResolveToMessage(e.Data, e.RemoteIP);
                if (m == null) return;
                if (m.Command == Commands.RecvConfirm)
                {
                    long pno = m.NormalMsg.TryParseToInt(0);
                    int pindex = m.ExtendMessage.TryParseToInt(0);
                    if (pno != 0)
                        this.Client.PopSendItemFromList(pno, pindex);
                    return;
                }
                //检查最近收到的消息队列里面是否已经包含了这个消息包，如果是，则丢弃
                if (!ReceivedQueue.Contains(m.PackageNo))
                {
                    ReceivedQueue.Enqueue(m.PackageNo);
                    if (ReceivedQueue.Count > 100) ReceivedQueue.Dequeue();

                    OnMessageReceived(new MessageEventArgs(m));
                }
                else
                    OnMessageDroped(new MessageEventArgs(m));
            }
        }

        public Msg ResolveToMessage(byte[] buffer, IPEndPoint remoteEndPoint)
        {
            if (buffer == null || buffer.Length < 0) return null;
            Msg m = null;
            if (MessagePacker.Test(buffer))
            {
                PacketNetWorkMsg pack = MessagePacker.Parse(buffer, remoteEndPoint);
                if (pack == null) return null;
                if (DetermineConfirm(pack))
                {
                    //发送确认标志
                    Msg cm = Helper.CreateRecivedCheck(remoteEndPoint, pack.PackageNo, pack.PackageIndex, _config);
                    Client.SendMsg(cm);
                }
                m = MessagePacker.TryToTranslateMessage(pack);
            }
            return m;
        }
        /// <summary>
        /// 检测是否需要发送回复包来确认收到
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static bool DetermineConfirm(PacketNetWorkMsg packet)
        {
            return packet.IsRequireReceiveCheck;
        }
        static bool DetermineConfirm(Msg message)
        {
            return message.IsRequireReceive;
        }
        #region 事件

        /// <summary>
        /// 接收到消息包（UDP）
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;
        SendOrPostCallback messageReceivedCallBack;
        /// <summary>
        /// 引发接收到消息包事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            if (MessageReceived == null) return;
            if (!SeiClient.NeedPostMessage)
            {
                MessageReceived(this, e);
            }
            else
            {
                if (messageReceivedCallBack == null) 
                    messageReceivedCallBack = s => MessageReceived(this, s as MessageEventArgs);

                SeiClient.SendSynchronizeMessage(messageReceivedCallBack, e);
            }
        }

        /// <summary>
        /// 消息将要发送事件
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSending;
        SendOrPostCallback messageSendingCallBack;
        /// <summary>
        /// 引发消息将要发送事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageSending(MessageEventArgs e)
        {
            if (MessageSending == null) return;

            if (!SeiClient.NeedPostMessage)
            {
                MessageSending(this, e);
            }
            else
            {
                if (messageSendingCallBack == null) 
                    messageSendingCallBack = s => MessageSending(this, s as MessageEventArgs);
                SeiClient.SendSynchronizeMessage(messageSendingCallBack, e);
            }
        }


        /// <summary>
        /// 消息已经发送事件
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSended;
        SendOrPostCallback messageSendedCall;
        /// <summary>
        /// 引发消息已经发送事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageSended(MessageEventArgs e)
        {
            if (MessageSended == null) return;

            if (!SeiClient.NeedPostMessage)
            {
                MessageSended(this, e);
            }
            else
            {
                if (messageSendedCall == null) 
                    messageSendedCall = s => MessageSended(this, s as MessageEventArgs);
                SeiClient.SendSynchronizeMessage(messageSendedCall, e);
            }
        }

        /// <summary>
        /// 重复收包然后丢包事件
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageDroped;
        /// <summary>
        /// 引发丢弃Msg事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageDroped(MessageEventArgs e)
        {
            if (MessageDroped == null) return;

            MessageDroped(this, e);
        }

        #endregion

    }
}
