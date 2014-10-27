using System;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            Cracking cracker = new Cracking();
            cracker.RunCracking();
            Console.WriteLine("Hej");
        }
    }
}
