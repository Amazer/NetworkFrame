using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NetServer
{
    class Program
    {
        class ClientState
        {
            public Socket socket;
            public byte[] readBuffer = new byte[1024];
        }
        static Dictionary<Socket, ClientState> clientDic = new Dictionary<Socket, ClientState>();
        static void Main(string[] args)
        {
            Console.WriteLine("hello world!");
            SocketFunc();
            Console.Read();
        }
        static void SocketFunc()
        {
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            listenfd.Listen(0);
            Console.WriteLine("【服务器】启动成功！");

            listenfd.BeginAccept(AcceptCallback, listenfd);
            

        }
        static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);
                ClientState cs = new ClientState();
                cs.socket = clientfd;
                clientDic.Add(clientfd, cs);
                clientfd.BeginReceive(cs.readBuffer, 0, 1024, 0, ReceiveCallback, cs.socket);
                Console.WriteLine("【服务器】Accetp！");

                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("accept error:" + ex.ToString());
            }

        }
        static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientfd = (Socket)ar.AsyncState;
                ClientState cs = clientDic[clientfd];
                int count = clientfd.EndReceive(ar);
                if(count ==0)
                {
                    clientfd.Close();
                    clientDic.Remove(clientfd);
                    Console.WriteLine("【Socket Close】");
                    return;
                }
                string recvStr = System.Text.Encoding.Default.GetString(cs.readBuffer, 0, count);
                Console.WriteLine("【服务器接收】"+recvStr);
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(recvStr);
                clientfd.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, clientfd);
                Console.WriteLine("【服务器发送】"+recvStr);

                clientfd.BeginReceive(cs.readBuffer, 0, 1024, 0, ReceiveCallback, clientfd);

            }
            catch(SocketException ex)
            {
                Console.WriteLine("receive error:" + ex.ToString());
            }

        }
        static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket connfd = (Socket)ar.AsyncState;
                int count = connfd.EndSend(ar);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("send error:" + ex.ToString());
            }

        }
    }
}
