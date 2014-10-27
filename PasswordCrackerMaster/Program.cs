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
        public static List<List<string>> splitList = new List<List<string>>();
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
            Send(socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecieveMessage, socket);
        }

        public static void SplitList()
        {
            string[] words = File.ReadAllLines("webster-dictionary.txt");
            List<List<string>> makeList = MakeList(words, 5000);
            foreach (List<string> list in makeList)
            {
                stack.Push(list);
            }
        }

        public static List<List<string>> MakeList(string[] words, int size)
        {
            int startIndex = 0;
            List<List<string>> result = new List<List<string>>();
            List<string> wordList = words.ToList();
            while (true)
            {
                if (wordList.Count - startIndex < size)
                {
                    List<string> smallList = wordList.GetRange(startIndex, wordList.Count - size);
                    result.Add(smallList);
                    break;
                }
                List<string> list = wordList.GetRange(startIndex, size);
                result.Add(list);
                startIndex += size;
            }
            return result;
        }

        private static void Send(Socket client)
        {
            try
            {
                if (stack.Count != 0)
                {
                    string fileName = "webster-dictionary.txt";
                    List<string> message = stack.Pop();
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    if (!File.Exists(fileName))
                    {
                        File.Create(fileName);
                    }
                    foreach (string words in message)
                    {
                        try
                        {
                            using (var file = new StreamWriter("webster-dictionary.txt", true))
                            {
                                file.Write(words + "\n");
                            }
                            File.WriteAllText(fileName, words);
                            Console.ReadLine();
                        }
                        catch (IOException)
                        {
                            Console.WriteLine("Klart nok");
                            
                        }
                    }
                    string fileSend = fileName;
                    var msg = Encoding.UTF8.GetBytes(fileSend);
                    byte[] byteMsg = msg;
                    client.BeginSend(byteMsg, 0, byteMsg.Length, SocketFlags.None, SendMessage, client);
                    _serverSocket.BeginAccept(RecievedCall, null);
                    client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecieveMessage, client);

                }
            }
            catch (SocketException)
            {

                Console.WriteLine("Error");
                
            }
        }

        private static void SendMessage(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket) ar.AsyncState;
                socket.EndSend(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Error2");
            }
        }

        private static void RecieveMessage(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket) ar.AsyncState;
                int recieved = socket.EndReceive(ar);
                byte[] dataBuf = new byte[recieved];
                Array.Copy(_buffer, dataBuf, recieved);
                string text = Encoding.ASCII.GetString(dataBuf);
                Console.WriteLine("Passwords Recieved: " + "\n" + text);
                if (text.Contains(text))
                {
                    Send(socket);
                }
                else
                {
                    Console.WriteLine("No more passwords");
                }
            }
            catch (SocketException)
            {

                Console.WriteLine("Client is offline");
                
            }
        }
    }
}
