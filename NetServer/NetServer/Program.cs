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
        static string sendStr = null;
        static void Main(string[] args)
        {
            Console.WriteLine("hello world!");
            SocketFunc();
            //            Console.Read();
        }
        static void SocketFunc()
        {
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            listenfd.Listen(0);
            Console.WriteLine("【服务器】启动成功！");
            List<Socket> checkList = new List<Socket>();
            while (true)
            {
                checkList.Clear();
                checkList.Add(listenfd);
                foreach (var v in clientDic.Values)
                {
                    checkList.Add(v.socket);
                }
                Socket.Select(checkList, null, null, 0);
                foreach (var v in checkList)
                {
                    if (v == listenfd)
                    {
                        ReadListenfd(v);
                    }
                    else
                    {
                        ReadClientfd(v);
                    }
                }
                System.Threading.Thread.Sleep(15);
            }
        }
        static void ReadListenfd(Socket socket)
        {
            Console.WriteLine("【服务器】Accetp！");
            Socket clientfd = socket.Accept();
            ClientState cs = new ClientState();
            cs.socket = clientfd;
            clientDic.Add(clientfd, cs);
        }
        static bool ReadClientfd(Socket socket)
        {
            ClientState cs = clientDic[socket];

            int count = 0;
            try
            {
                count = cs.socket.Receive(cs.readBuffer);
            }
            catch (SocketException ex)
            {

                Console.WriteLine("ReadClientfd exception:" + ex.ToString());
            }
            if (count == 0)
            {
                socket.Close();
                clientDic.Remove(socket);
                Console.WriteLine("【Socket Close】");
                return false;
            }
            string recvStr = System.Text.Encoding.UTF8.GetString(cs.readBuffer, 2, count - 2);
            Console.WriteLine("【服务器接收】" + recvStr);

            sendStr = socket.RemoteEndPoint.ToString() + ":" + recvStr;
            byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
            byte[] sendLength = BitConverter.GetBytes((Int16)sendBytes.Length);
            sendBytes = sendLength.Concat(sendBytes).ToArray();

            Console.WriteLine("【服务器】 转发:" + Encoding.UTF8.GetString(sendBytes));
            foreach (var v in clientDic.Values)
            {
                int sendCount = v.socket.Send(sendBytes);
                //                Console.WriteLine("【服务器】 sendCount:" + sendCount);
            }
            return true;
        }
    }
}
