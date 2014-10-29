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
        
        static void Main(string[] args)
        {
            Master master = new Master(8080, 1000);
            master.Run();
        }

        
    }
}
