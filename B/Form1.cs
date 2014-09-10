using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Netframe.Core;
using Netframe.Event;
using Netframe;
using Netframe.Model;

namespace B
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            Config cfg=SeiClient.GetDefaultConfig();
            cfg.Port = 8888;
            UDPThread udp = new UDPThread(cfg);
            MsgTranslator tran = new MsgTranslator(udp, cfg);
            tran.MessageReceived += tran_MessageReceived;
        }
        void tran_MessageReceived(object sender, MessageEventArgs e)
        {
            System.Console.WriteLine(e.msg.Command);
            AddServerMessage(e.msg.Command.ToString());
            AddServerMessage(e.msg.PackageNo.ToString());
            AddServerMessage(e.msg.NormalMsg.ToString());
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

    }
}
