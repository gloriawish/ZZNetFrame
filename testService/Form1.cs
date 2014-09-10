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
using Netframe.Tool;

namespace testService
{
    public partial class Form1 : Form
    {
        MsgTranslator tan;
        LogInfo log;
        public Form1()
        {
            log = new LogInfo("log.txt");
            InitializeComponent();
            UDPThread client = new UDPThread(6666);
            tan = new MsgTranslator(client);
            tan.MessageReceived += MessageReceived;
            tan.DebugEvent += Debug;
        }
        //消息收到后的事件
        private void MessageReceived(object sender, MessageEventArgs e)
        {
            AddMessage(e.msg.NormalMsg);
        }
        private delegate void MsgDelegate(string message);
        public void AddMessage(string message)
        {
            if (richTextBox1.InvokeRequired)//不能访问就创建委托
            {
                MsgDelegate d = new MsgDelegate(AddMessage);
                richTextBox1.Invoke(d, new object[] { message });
            }
            else
            {
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
        }

        //Debug
        private void Debug(object sender, DebugEventArgs e)
        {
            log.WriteLog(e.DebugMsg);
            AddDebugMessage(e.DebugMsg);
        }
        private delegate void DebugMsgDelegate(string message);
        public void AddDebugMessage(string message)
        {
            if (richTextBox2.InvokeRequired)//不能访问就创建委托
            {
                DebugMsgDelegate d = new DebugMsgDelegate(AddDebugMessage);
                richTextBox2.Invoke(d, new object[] { message });
            }
            else
            {
                richTextBox2.AppendText(message + Environment.NewLine);
                richTextBox2.ScrollToCaret();
            }
        }
    }
}
