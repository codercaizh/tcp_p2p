using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TCP穿透客户端
{
    public partial class Form1 : Form
    {
        IPEndPoint server,local,remote;
        Socket s,c;
        Thread listenThread, receiveThread;
        public Form1()
        {
            InitializeComponent();
            server = new IPEndPoint(IPAddress.Parse("192.168.199.163"), 2222);//服务端的IP和监听端口
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           s　=　new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           s.ExclusiveAddressUse = false;
           s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
           try
           {
               s.Connect(server);
               st.Text = st.Text + "发送测试报文\r\n";
               s.Send(System.Text.Encoding.Default.GetBytes("test"));
           }
           catch
           {
               st.Text = st.Text + "服务端不在线\r\n";
               return;
           }
          local = (IPEndPoint)s.LocalEndPoint;
          listenThread= new Thread(listen);
          listenThread.Start();
          receiveThread=new Thread(receive);
          receiveThread.Start();          
        }

        private void receive()
        {
            byte[] bs = new byte[24];
            try
            {
                s.Receive(bs);
            }
            catch
            {
                return;
            }
            String rec = System.Text.Encoding.Default.GetString(bs).Trim('\0');
            if ("test".Equals(rec))
            {
                st.Text = st.Text + "已接受到服务器回传的测试报文，此路径正常\r\n";
                receive();
                return;
            }
            if ("-1".Equals(rec))
            {
                st.Text = st.Text + "对方主机未上线，稍后再试\r\n";
                receive();
                return;
            }
                remote = new IPEndPoint(IPAddress.Parse(rec.Split(':')[0]), Convert.ToInt32(rec.Split(':')[1]));
                c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                c.ExclusiveAddressUse = false;
                c.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                c.Bind(local);
                st.Text = st.Text + "正在尝试使用本地的" + local.Port + "连接" + remote.Address + ":" + remote.Port + "\r\n";
                try
                {
                    c.Connect(remote);
                    s.Close();
                }
                catch
                {
                    st.Text = st.Text + "尝试主动连接对方失败，打洞完毕\r\n";
                    st.Text = st.Text + "洞口信息:本机的" + local.Port + "到远程主机的" + remote.Address + ":" + remote.Port + "\r\n";
                    st.Text = st.Text + "正在请求服务器使远程主机发起连接";
             
                    s.Send(System.Text.Encoding.Default.GetBytes("0"));
                    receive();
                    return;
                }
                listenThread.Abort();
                st.Text = st.Text + "成功连接远程主机,开始进入会话\r\n";
                st.Text = st.Text + "本地端口号:" + local.Port + "\t\t远程主机地址:" + c.RemoteEndPoint.ToString() + "\r\n";
                while (true)
                {
                    byte[] bytes = new byte[60];
                    c.Receive(bytes);
                    String data = System.Text.Encoding.Default.GetString(bytes).Trim('\0');
                    st.Text = st.Text + "接受文字:" + data + "\r\n";
                }
            
        }
        private void listen()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.ExclusiveAddressUse = false;
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Bind(local);
            st.Text = st.Text + "正在监听"+local.Port+"端口\r\n";
            client.Listen(10);
            c = client.Accept();
            s.Close();
            st.Text = st.Text + "有新的连接加入,开始进入会话\r\n";
            st.Text = st.Text + "本地端口号:" + local.Port + "\t\t远程主机地址:" + c.RemoteEndPoint.ToString() + "\r\n";
            while (true)
            {
                String time=DateTime.Now.ToString();
                st.Text = st.Text + "发送文字:" + time + "\r\n";
                c.Send(System.Text.Encoding.Default.GetBytes(time));
                Thread.Sleep(1000);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            s.Send(System.Text.Encoding.Default.GetBytes("1"));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listenThread.Abort();
            receiveThread.Abort();
            Application.Exit();
        }
    }
}
