using System;
using System.Collections.Generic;
using MumbleProto;

namespace Protocols.Mumble
{
    public class MumbleClient : MumbleConnection
    {
        private readonly Dictionary<UInt32, MumbleChannel> channels = new Dictionary<UInt32, MumbleChannel>();

        public Dictionary<UInt32, MumbleChannel> Channels
        {
            get
            {
                return channels;
            }
        }

        public MumbleChannel RootChannel { get; internal set; }

        private readonly Dictionary<UInt32, MumbleUser> users = new Dictionary<UInt32, MumbleUser>();

        public Dictionary<UInt32, MumbleUser> Users
        {
            get
            {
                return users;
            }
        }

        public MumbleUser User { get; private set; }

        public string Version { get; private set; }

        public uint ServerVersion { get; private set; }
        public string ServerOS { get; private set; }
        public string ServerOSVersion { get; private set; }
        public string ServerRelease { get; set; }

        public string WelcomeText { get; private set; }
        public uint MaxBandwith { get; private set; }


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

            if (proto != null) { proto.HandleMessage(this); }
        }

        public void Update(MumbleProto.Version message)
        {
            ServerOS = message.os;
            ServerOSVersion = message.os;
            ServerRelease = message.release;
            ServerVersion = message.version;
        }

        public void Update(ServerSync message)
        {
            if (message.sessionSpecified) { User = users[message.session]; }
            if (message.max_bandwidthSpecified) { MaxBandwith = message.max_bandwidth; }
            if (message.welcome_textSpecified) { WelcomeText = message.welcome_text; }
        }

        private UInt64 sequence = 1;

        internal UInt64 NextSequence()
        {
            return sequence += 2;
        }
    }
}
