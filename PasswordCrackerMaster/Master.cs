using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerMaster
{
    class Master
    {
        Stopwatch watch = new Stopwatch();
        public int Port { get; private set; }
        public int BlockSize { get; private set; }
        private List<List<String>> _listlist;
        private int _index = 0;
        private int _howmanySlavesCameBack = 0;
        public List<string> userInfo = new List<string>();


        public Master(int port, int blockSize)
        {
            Port = port;
            BlockSize = blockSize;
        }

        public void Run()
        {
            _listlist = MakeList(@"c:\temp\webster-dictionary.txt", BlockSize);
            Console.WriteLine(String.Join(", ", _listlist));

            IPAddress adr = IPAddress.Parse("10.154.1.141");
            TcpListener server = new TcpListener(adr, Port);
            server.Start();
            while (true)
            {
                Console.WriteLine("Master: " + "number of slaves: " + _howmanySlavesCameBack + " " + "number of lists: " + _index);
                Socket client = server.AcceptSocket();
                if (_index == _listlist.Count && _index == _howmanySlavesCameBack)
                {
                    ShutDown(client);
                    break;
                }
                HandleClient(client);
                client.Close();
            }
            watch.Stop();
            Console.WriteLine("Time elapsed: {0}", watch.Elapsed);
            foreach (string users in userInfo)
            {
                Console.WriteLine("Slaves has found the following users and passwords: " + users);
            }
            server.Stop();
        }

        private void ShutDown(Socket client)
        {
            using (StreamWriter toClient = new StreamWriter(new NetworkStream(client)))
            {
                toClient.WriteLine("ShutDown Sockets");
            }
        }

        private void HandleClient(Socket client)
        {
            using (StreamReader fromClient = new StreamReader(new NetworkStream(client)))
            {
                String request = fromClient.ReadLine();
                Console.WriteLine("Master request: " + request);
                if (request.StartsWith("GET"))
                {
                    watch.Start();
                    DoGet(client);
                }
                else if (request.StartsWith("FOUND"))
                {
                    DoFound(fromClient);
                }
                else if (request.StartsWith("NOT FOUND"))
                {
                    DoNotFound();
                }
            }
        }

        private void DoGet(Socket client)
        {
            using (StreamWriter toClient = new StreamWriter(new NetworkStream(client)))
            {
                if (_index < _listlist.Count)
                {
                    foreach (String word in _listlist[_index])
                    {
                        toClient.WriteLine(word);
                    }
                    toClient.Flush();
                    _index++;
                }
            }
        }

        private void DoFound(StreamReader fromClient)
        {
            String username = fromClient.ReadLine();
            String password = fromClient.ReadLine();
            string users = username + password;
            string[] trysplit = users.Split(',');
            foreach (string parts in trysplit)
            {
                userInfo.Add(parts);
            }
            userInfo.RemoveAll(string.IsNullOrWhiteSpace);
            Console.WriteLine(users);
            _howmanySlavesCameBack++;
        }

        private void DoNotFound()
        {
            _howmanySlavesCameBack++;
        }

        public static List<List<string>> MakeList(string fileName, int size)
        {
            string[] array = File.ReadAllLines(fileName);
            int startIndex = 0;
            List<List<string>> result = new List<List<string>>();
            List<string> wordList = array.ToList();
            while (true)
            {
                if (wordList.Count - startIndex < size)
                {
                    List<string> smallList = wordList.GetRange(wordList.Count - startIndex, size);
                    result.Add(smallList);
                    break;
                }
                List<string> list = wordList.GetRange(startIndex, size);
                result.Add(list);
                startIndex += size;
            }
            return result;
        }
    }
}
