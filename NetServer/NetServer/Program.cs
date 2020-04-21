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

            while(true)
            {
                Socket connfd = listenfd.Accept();
                Console.WriteLine("【服务器】Accetp！");

                byte[] readBuff = new byte[1024];
                int count = connfd.Receive(readBuff);
                string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
                Console.WriteLine("【服务器接收】"+recvStr);

                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(recvStr);
                connfd.Send(sendBytes);
            }
        }
    }
}
