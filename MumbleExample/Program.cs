using System;
using Protocol.Mumble;

namespace MumbleExample
{
    class Program
    {
        static void Main(string[] args)
        {

            var client = new MumbleClient("Mumble.net", "ciphershed.org", "Test");

            client.Connect();

            Console.ReadLine();
            Console.WriteLine(client.RootChannel.Tree());
            Console.ReadLine();

        }
    }
}
