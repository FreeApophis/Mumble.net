using System;
using System.Collections.Generic;
using System.Linq;

namespace Protocol.Mumble
{
    public class MumbleClient : MumbleConnection
    {
        private readonly Dictionary<UInt32, MumbleChannel> _channels = new Dictionary<UInt32, MumbleChannel>();

        public Dictionary<UInt32, MumbleChannel> Channels => _channels;

        public MumbleChannel RootChannel { get; internal set; }

        public Dictionary<UInt32, MumbleUser> Users { get; } = new Dictionary<UInt32, MumbleUser>();

        public MumbleUser ClientUser { get; private set; }

        public string Version { get; }

        public uint ServerVersion;
        public string ServerOS { get; private set; }
        public string ServerOSVersion { get; private set; }
        public string ServerRelease { get; set; }

        public string WelcomeText { get; private set; }
        public uint MaxBandwith { get; private set; }

        public event EventHandler<MumblePacketEventArgs> OnConnected;
        public event EventHandler<MumblePacketEventArgs> OnTextMessage;


        public MumbleClient(string version, string host, string username, int port = 64738) :
            base(host, username, port)
        {
            Version = version;
            OnPacketReceived += ProtocolHandler;
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

            proto?.HandleMessage(this);
        }

        public void Update(Version message)
        {
            ServerOS = message.os;
            ServerOSVersion = message.os;
            ServerRelease = message.release;
            ServerVersion = message.version;
        }

        public void Update(ServerSync message)
        {
            if (message.sessionSpecified) { ClientUser = Users[message.session]; }
            if (message.max_bandwidthSpecified) { MaxBandwith = message.max_bandwidth; }
            if (message.welcome_textSpecified) { WelcomeText = message.welcome_text; }

            DispatchEvent(this, OnConnected, new MumblePacketEventArgs(message));
        }

        public void Update(TextMessage message)
        {
            DispatchEvent(this, OnTextMessage, new MumblePacketEventArgs(message));
        }

        public void SendTextMessageToUser(string message, MumbleUser user)
        {
            SendTextMessage(message, null, null, Enumerable.Repeat(user, 1));
        }

        public void SendTextMessageToChannel(string message, MumbleChannel channel, bool recursive)
        {
            if (recursive)
            {
                SendTextMessage(message, null, Enumerable.Repeat(channel, 1), null);
            }
            else
            {
                SendTextMessage(message, Enumerable.Repeat(channel, 1), null, null);
            }
        }

        public void SwitchChannel(MumbleChannel channel)
        {
            SendUserState(channel);
        }

        public MumbleChannel FindChannel(string name)
        {
            return _channels.Values.FirstOrDefault(channel => channel.Name == name);
        }

        public MumbleUser FindUser(uint id)
        {
            return Users.ContainsKey(id) ? Users[id] : null;
        }

        private UInt64 _sequence = 1;

        internal UInt64 NextSequence()
        {
            return _sequence += 2;
        }

    }
}
