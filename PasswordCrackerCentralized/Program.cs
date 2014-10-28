using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using PasswordCrackerCentralized.model;

namespace PasswordCrackerCentralized
{
    internal class Slave
    {
        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        public class AsynchronousClient
        {
            private const int Port = 65080;

            private static readonly ManualResetEvent ConnectDone = new ManualResetEvent(false);
            private static readonly ManualResetEvent SendDone = new ManualResetEvent(false);
            private static readonly ManualResetEvent ReceiveDone = new ManualResetEvent(false);

            private static String response = String.Empty;
            public static Cracking crack = new Cracking();

            public static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            private static void Main(string[] args)
            {
                Console.Title = "Client";
                StartClient();
            }

            private static void StartClient()
            {
                try
                {
                    IPAddress ip = IPAddress.Parse("10.154.1.37");
                    IPEndPoint remoteEP = new IPEndPoint(ip, Port);

                    client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                    ConnectDone.WaitOne();
                    List<UserInfoClearText> list = crack.RunCracking();
                    string answer = string.Empty;

                    for (int i = 0; 1 < list.Count; i++)
                    {
                        answer = list[i].ToString();
                    }
                    Send(client, answer);
                    SendDone.WaitOne();

                    Receive(client);
                    ReceiveDone.WaitOne();
                    crack.RunCracking();

                    Console.WriteLine("Response received : {0}", response);
                    byte[] words = Encoding.ASCII.GetBytes(response);
                    SaveFile(words);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private static void SaveFile(byte[] words)
            {
                string receivedPart = "C://Users//Morten//Documents//Visual Studio 2013//Projects//password//PasswordCracker-1//temp//";
                int fileNameLen = BitConverter.ToInt32(words, 0);
                string fileName = Encoding.ASCII.GetString(words, 4, fileNameLen);
                Console.WriteLine("Client:{0} connected & File {1} started received.", client.RemoteEndPoint, fileName);
                if (File.Exists(receivedPart + fileName))
                {
                    File.Delete(receivedPart + fileName);
                }
                BinaryWriter bWrite = new BinaryWriter(File.Open(receivedPart + fileName, FileMode.Append));
                bWrite.Write(words);
                Console.WriteLine("File: {0} received & saved at path: {1}", fileName, receivedPart);

                bWrite.Close();
                client.Close();
                Console.ReadLine();
            }

            private static void Receive(Socket client)
            {
                try
                {
                    StateObject state = new StateObject();
                    state.workSocket = client;

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private static void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    StateObject state = (StateObject)ar.AsyncState;
                    Socket client = state.workSocket;

                    int bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        if (state.sb.Length > 1)
                        {
                            response = state.sb.ToString();
                        }
                        ReceiveDone.Set();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private static void Send(Socket client, string answer)
            {
                byte[] byteData = Encoding.ASCII.GetBytes(answer);
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
            }

            private static void SendCallback(IAsyncResult ar)
            {
                try
                {
                    Socket clint = (Socket)ar.AsyncState;
                    int bytesSent = clint.EndSend(ar);
                    Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                    SendDone.Set();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private static void ConnectCallback(IAsyncResult ar)
            {
                try
                {
                    Socket clint = (Socket)ar.AsyncState;
                    clint.EndConnect(ar);
                    Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                    ConnectDone.Set();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
