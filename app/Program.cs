using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocols.Mumble;

namespace Mumble.net.app
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MumbleClient("Mumble.net");

            client.Connect();

            System.Console.ReadLine();
            System.Console.WriteLine(client.RootChannel.Tree());
            System.Console.ReadLine();

        }
    }
}
