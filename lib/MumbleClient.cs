using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MumbleProto;

namespace Protocols.Mumble
{
    public class MumbleClient : MumbleConnection
    {
        private Dictionary<UInt32, MumbleChannel> channels = new Dictionary<UInt32, MumbleChannel>();

        public Dictionary<UInt32, MumbleChannel> Channels
        {
            get
            {
                return channels;
            }
        }

        public MumbleChannel RootChannel { get; internal set; }

        private Dictionary<UInt32, MumbleUser> users = new Dictionary<UInt32, MumbleUser>();

        public Dictionary<UInt32, MumbleUser> Users
        {
            get
            {
                return users;
            }
        }

        public string Version { get; private set; }

        public uint serverVersion;
        public string ServerOS { get; private set; }
        public string ServerOSVersion { get; private set; }
        public string ServerRelease { get; set; }

        public MumbleClient(string version)
        {
            Version = version;
            PacketReceivedEvent += ProtocolHandler;
        }

        public new void Connect()
        {
            base.Connect();

            SendVersion(Version);
            SendAuthenticate();
        }

        private void ProtocolHandler(object sender, MumblePacketEventArgs args)
        {
            var proto = args.Message as IProtocolHandler;

            proto.HandleMessage(this);
        }

        public void Update(MumbleProto.Version message)
        {
            ServerOS = message.os;
            ServerOSVersion = message.os;
            ServerRelease = message.release;
            serverVersion = message.version;
        }
    }
}
