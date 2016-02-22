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

namespace 服务端
{
    public partial class Form1 : Form
    {
        IPEndPoint local;
        Socket localsocket,SocketA,SocketB;
        char choose;
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
           
        }
        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(listen).Start();
        }
        private void listen()
        {
            local = new IPEndPoint(IPAddress.Parse("192.168.199.163"), 2222);//本地的IP以及要进行监听的端口号
            localsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            localsocket.ExclusiveAddressUse = false;
            localsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            localsocket.Bind(local);
            localsocket.Listen(10);
            st.Text = st.Text + "启动服务成功！\r\n";
            SocketA = localsocket.Accept();
            st.Text = st.Text + "主机A已经加入\r\n";
            choose = 'A';
            new Thread(accept).Start();
            localsocket.Listen(10);
            SocketB = localsocket.Accept();
            st.Text = st.Text + "主机B已经加入\r\n";
            choose = 'B';
            new Thread(accept).Start();

        }
     
        private void accept()
        {
            Socket client;
            if (choose.Equals('A')) client = SocketA;
            else if (choose.Equals('B')) client = SocketB;
            else return;
            while (true)
            {
                byte[] bytes=new byte[23];
                client.Receive(bytes);
                String rec = System.Text.Encoding.Default.GetString(bytes).Trim('\0');
                if ("test".Equals(rec))
                {
                    client.Send(System.Text.Encoding.Default.GetBytes("test"));
                }
                else if ("1".Equals(rec))
                {
                    if(SocketA==null||SocketB==null)
                        client.Send(System.Text.Encoding.Default.GetBytes("-1"));
                    else if (SocketA == client)
                    {
                        client.Send(System.Text.Encoding.Default.GetBytes(SocketB.RemoteEndPoint.ToString()));
                        st.Text = st.Text + "收到主机A的请求，将主机B的地址发给主机A\r\n";
                    }
                    else if (SocketB == client)
                    {
                        client.Send(System.Text.Encoding.Default.GetBytes(SocketA.RemoteEndPoint.ToString()));
                        st.Text = st.Text + "收到主机B的请求，将主机A的地址发给主机B\r\n";
                    }
                }
                else if ("0".Equals(rec))
                {
                    if (SocketA == null || SocketB == null)
                        client.Send(System.Text.Encoding.Default.GetBytes("-1"));
                    else if (SocketA == client)
                    {
                        SocketB.Send(System.Text.Encoding.Default.GetBytes(client.RemoteEndPoint.ToString()));
                        st.Text = st.Text + "收到主机A的请求，将主机A的地址发给主机B\r\n";
                    }
                    else if (SocketB == client)
                    {
                        SocketA.Send(System.Text.Encoding.Default.GetBytes(client.RemoteEndPoint.ToString()));
                        st.Text = st.Text + "收到主机B的请求，将主机B的地址发给主机A\r\n";
                    }
                }
            }
        }
    }
}
