using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Netframe.Tool;
using Netframe.Model;
using System.Net;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("name","zhujun");
            dic.Add("age", 19);
            richTextBox1.Text = Json.JsonSerializerBySingleData(dic);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> dic = Json.JsonDeserializeBySingleData<Dictionary<string, object>>(richTextBox1.Text);

            MessageBox.Show(dic["name"]+":"+dic["age"]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IPEndPoint host = new IPEndPoint(IPAddress.Any, 8888);
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 7777);
            Msg m = new Msg(remote,"zz","zhujun",Commands.Entry,"test1","test2");
            m.PackageNo = 12345678;
            PacketNetWorkMsg[] udps= MessagePacker.BuildNetworkMessage(m);
            PacketNetWorkMsg p=MessagePacker.Parse(udps[0].Data, null);
            Msg ms = MessagePacker.TryToTranslateMessage(p);
        }
    }
}
