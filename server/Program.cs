using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Core;
using Netframe.Event;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            UDPThread udp = new UDPThread(7777);
            MsgTranslator tran = new MsgTranslator(udp);
            tran.MessageReceived += tran_MessageReceived;
        }
        static void tran_MessageReceived(object sender, MessageEventArgs e)
        {
            System.Console.WriteLine(e.msg.Command);
        }
    }
}
