using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace ChatServer
{
    public class Server
    {
        public static Dictionary<string, TcpClient> ClientList = new Dictionary<string, TcpClient>();
        public static Dictionary<string, string> ColorCollection = new Dictionary<string, string>();

        public enum Colors
        {
            Blue = 0,
            Green = 1,
            Cyan = 2,
            Red = 3,
            Magenta = 4,
            Yellow = 5,
        }

        static void Main(string[] args)
        {
            var ip = GetIP();
            var serverSocket = new TcpListener(ip, 52100);
            var clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine(string.Format("Chat server started @ {0}:{1}",ip.ToString(),52100));
            while (true)
            {
                string dataFromClient;

                counter++;
                clientSocket = serverSocket.AcceptTcpClient();

                var bytesFrom = new byte[65536];
                try
                {
                    var networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    var rand = new Random().Next(0, 6);
                    var color = ((Colors)rand).ToString();
                    ClientList.Add(dataFromClient, clientSocket);
                    ColorCollection.Add(dataFromClient, color);
                    Broadcast(string.Format("[COLOR:{0}@{1}] Joined", color, dataFromClient), dataFromClient, false);
                    Console.WriteLine(string.Format("{0}({1}) Joined the chat room", dataFromClient, color));
                    var client = new HandleClient(clientSocket, dataFromClient, ClientList, ColorCollection);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("Exiting. Press any key to continue...");
            Console.ReadKey();

        }


        public static void Broadcast(string message, string userName, bool flag)
        {
            foreach(var item in ClientList)
            {
                if (!item.Value.Client.Connected)
                    continue;
                TcpClient broadcastSocket;
                byte[] broadcastBytes;

                broadcastSocket = item.Value;
                var color = ColorCollection[userName];
                var broadcastStream = broadcastSocket.GetStream();

                if (flag)
                    broadcastBytes = Encoding.ASCII.GetBytes(string.Format("[COLOR:{0}@{1}] says: [COLOR:{0}@{2}] ({3})", color, userName, message, DateTime.Now.ToString()));
                else
                    broadcastBytes = Encoding.ASCII.GetBytes(message);

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }
        public static IPAddress GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach(var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }
            return null;
        }


    }

    public class HandleClient
    {
        TcpClient _clientSocket;
        string _clientNumber;
        Dictionary<string, TcpClient> _clientList;
        Dictionary<string, string> _colorCollection;

        public HandleClient(TcpClient inClientSocket, string clientNumber, Dictionary<string, TcpClient> clientList, Dictionary<string, string> ColorCollection)
        {
            _clientSocket = inClientSocket;
            _clientNumber = clientNumber;
            _clientList = clientList;
            _colorCollection = ColorCollection;
            var th = new Thread(chat);
            th.Start();
        }

        private void chat()
        {
            var requestCount = 0;
            var bytesFrom = new byte[65536];
            string dataFromClient;
            byte[] sendBytes;
            string serverResponse;
            string rCount;

            while (true)
            {
                try
                {
                    requestCount++;
                    var networkStream = _clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, _clientSocket.ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(string.Format("From client {0}: {1}", _clientNumber, dataFromClient));
                    rCount = Convert.ToString(requestCount);

                    Server.Broadcast(dataFromClient, _clientNumber, true);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    var color = _colorCollection[_clientNumber];
                    Server.Broadcast(string.Format("[COLOR:{0}@{1}] Has left the chat", color, _clientNumber), _clientNumber, false);
                    break;
                }
            }

        }
    }
}
