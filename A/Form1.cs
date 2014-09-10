using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Netframe.Core;
using System.Net;
using Netframe.Model;
using Netframe.Event;
using Netframe;
using Netframe.Tool;

namespace A
{
    public partial class Form1 : Form
    {
        MsgTranslator tran = null;
        public Form1()
        {
            InitializeComponent();
            Config cfg = SeiClient.GetDefaultConfig();
            cfg.Port = 7777;
            UDPThread udp = new UDPThread(cfg);
            tran = new MsgTranslator(udp, cfg);
            tran.MessageReceived += tran_MessageReceived;
            
        }
        void tran_MessageReceived(object sender, MessageEventArgs e)
        {
            System.Console.WriteLine(e.msg.Command);
            AddServerMessage(e.msg.Command.ToString());
            AddServerMessage(e.msg.PackageNo.ToString());
            AddServerMessage(e.msg.NormalMsg.ToString());
        }
        private void button1_Click(object sender, EventArgs e)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse("58.198.176.217"), 8888);
            Msg m = new Msg(remote, "zz", "zhujun", Commands.Entry, "test11111111111111111111111111111", "test2333333333");
            m.IsRequireReceive = true;
            m.PackageNo =Msg.GetRandomNumber();
            tran.Send(m);
        }

        private delegate void MessageDelegate(string message);
        public void AddServerMessage(string message)
        {
            if (richTextBox1.InvokeRequired)//不能访问就创建委托
            {
                MessageDelegate d = new MessageDelegate(AddServerMessage);
                richTextBox1.Invoke(d, new object[] { message });
            }
            else
            {
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s=Tool.GetWeather("重庆");
        }
    }
}
