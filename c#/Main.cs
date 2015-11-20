using System;
using System.Net;

using Miqi.Net;

namespace Miqi
{
    class Program
    {
        static void Main()
        {
            MiqiServer server = new MiqiServer(54321);
            server.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            server.Stop();
        }
    }
}
