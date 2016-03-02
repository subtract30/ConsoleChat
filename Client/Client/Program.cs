using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace ChatClient
{
    public class Client
    {
        public static TcpClient ClientSocket = new TcpClient();
        public static NetworkStream ServerStream = default(NetworkStream);
        public static string ReadData;
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
            Console.WriteLine("Enter an IP Address...");
            var ip = Console.ReadLine();
            ClientSocket.Connect(ip, 52100);
            ReadData = "Connected to chat server...";
            

            ServerStream = ClientSocket.GetStream();
            var th = new Thread(getMessage);
            th.Start();

            Console.WriteLine("Enter your name on the chat server");

            var input = Console.ReadLine() + "$";
            var outStream = Encoding.ASCII.GetBytes(input);
            ServerStream.Write(outStream, 0, outStream.Length);
            ServerStream.Flush();

            while (true)
            {
                input = Console.ReadLine() + "$";
                if (input.Trim().ToLower() == "/logout$")
                    break;
                outStream = Encoding.ASCII.GetBytes(input);
                ServerStream.Write(outStream, 0, outStream.Length);
                ServerStream.Flush();
            }
            ClientSocket.Close();

        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (ClientSocket.Client.Connected)
                ClientSocket.Close(); 
        }
        private static void getMessage()
        {
            while (true)
            {
                if (!ClientSocket.Client.Connected)
                    break;
                ServerStream = ClientSocket.GetStream();
                var bufferSize = 0;
                var inStream = new byte[65536];
                bufferSize = ClientSocket.ReceiveBufferSize;
                ServerStream.Read(inStream, 0, bufferSize);
                var returnData = Encoding.ASCII.GetString(inStream);
                ReadData = returnData.TrimEnd('\0');
                var color = "White";
                var clipData = "";
                var outPut = "";
                var consoleColor = ConsoleColor.White;
                while (!String.IsNullOrEmpty(ReadData))
                {
                    if(ReadData.Substring(0,1) == "[")
                    {
                        color = ReadData.Substring(ReadData.IndexOf(":")+1, ReadData.IndexOf("@")-ReadData.IndexOf(":")-1);
                        clipData = ReadData.Substring(0, ReadData.IndexOf("]")+1);
                        outPut = ReadData.Substring(ReadData.IndexOf(":")+ 1 + color.Length + 1, ReadData.IndexOf("]") - ReadData.IndexOf("@")-1);
                    }
                    else
                    {
                        color = "White";
                        clipData = ReadData.Contains("[") ? ReadData.Substring(0, ReadData.IndexOf("[")) : ReadData.Substring(0, ReadData.Length);
                        outPut = clipData;
                    }
                    ReadData = ReadData.Replace(clipData, "");

                    Enum.TryParse<ConsoleColor>(color, out consoleColor);
                    Console.ForegroundColor = consoleColor;
                    Console.Write(outPut);
                }
                Console.Write("\n");
            }
        }
    }
}
