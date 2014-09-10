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
using Netframe.Model;
using System.Net;
using Netframe.Tool;

namespace testClient
{
    public partial class Form1 : Form
    {
        MsgTranslator tan;
        public Form1()
        {
            InitializeComponent();
            UDPThread client = new UDPThread(textBox1.Text,6666);
            tan = new MsgTranslator(client);
            tan.MessageReceived += MessageReceived;
            tan.DebugEvent += Debug;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPEndPoint dip=new IPEndPoint(IPAddress.Parse(textBox1.Text),6666);
            Msg msg = new Msg(dip, "amy", "zz", Commands.Join, 0, textBox2.Text, "");
            msg.IsRequireReceive = true;
            tan.SendMsg(msg);
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

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("name", "zhujun");
            dic.Add("age",19);
            richTextBox2.Text =Json.DictionaryToJson(dic);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            User u = Json.JsonDeserializeBySingleData<User>(richTextBox2.Text);

            MessageBox.Show(u.name+":"+u.age);
        }

    }
}
