using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace Server
{
    class Program
    {
        static Server server = new Server();

        static void Main(string[] args)
        {
            //var clientExecutable = @"E:\Dropbox\!Coding\All_in_one\Proiznoshator\Client\bin\Debug\Client.exe";

            //Process.Start(clientExecutable);

            Console.Title = "Server";

            server.SetupServer();
            Console.ReadLine();
        }
    }
}
