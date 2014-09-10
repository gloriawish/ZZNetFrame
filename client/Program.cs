using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netframe.Core;
using Netframe.Event;
using Netframe.Model;
using System.Net;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            UDPThread udp = new UDPThread("58.198.176.217", 7777);
            MsgTranslator tran = new MsgTranslator(udp);
            tran.MessageReceived += tran_MessageReceived;
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 7777);
            Msg m = new Msg(remote, "zz", "zhujun", Commands.Join, "test1", "test2");
            m.PackageNo = 12345678;
            tran.Send(m);
        }

        static void tran_MessageReceived(object sender, MessageEventArgs e)
        {
            System.Console.WriteLine(e.msg.Command);
        }
    }
}
