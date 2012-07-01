using System;
using Protocols.Mumble;

namespace Mumble.net.app
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MumbleClient("Mumble.net");

            client.Connect();

            Console.ReadLine();
            Console.WriteLine(client.RootChannel.Tree());
            Console.ReadLine();

        }
    }
}
