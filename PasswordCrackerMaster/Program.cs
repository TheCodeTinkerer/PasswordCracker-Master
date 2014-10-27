using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerMaster
{
    class Program
    {
        private static IPAddress ip = IPAddress.Parse("localhost");
        private static Stack<List<string>> stack = new Stack<List<string>>();
        private static byte[] _buffer = new byte[1024];
        private static List<Socket> _clientSocket = new List<Socket>();  
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine();
        }

        private static void SetupServer()
        {
            try
            {
                SplitList();
                _serverSocket.Bind(new IPEndPoint(ip, 65080));
                _serverSocket.Listen(5);
                Console.WriteLine("Server Started");
                _serverSocket.BeginAccept(new AsyncCallback(RecievedCall), null);
            }
            catch (SocketException)
            {
                
                throw new SocketException();
            }
        }

        private static void RecievedCall(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSocket.Add(socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecievedCall, socket);
        }

        private static void SplitList()
        {
            string[] words = File.ReadAllLines("webster-dictionary.txt");
            List<List<string>> makeList = MakeList(words, 5000);
            foreach (List<string> list in makeList)
            {
                stack.Push(list);
            }
        }

        private static void Send()
        {
            try
            {
                if (stack.Count != 0)
                {
                    string fileName = "webster-dictionary.txt";
                    List<string> message = stack.Pop();
                    

                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
