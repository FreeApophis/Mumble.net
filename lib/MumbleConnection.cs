using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ProtoBuf;

namespace Protocol.Mumble
{
    public class MumbleConnection
    {
        private int port;
        private string host;
        private bool connected;

        private uint mumbleVersion;
        private string username;

        private TcpClient tcpClient;
        private SslStream sslStream;

        private Thread listenThread;
        private Thread pingerThread;

        #region Public events

        public event EventHandler<MumblePacketEventArgs> OnPacketReceived;

        protected void DispatchEvent<T>(object sender, EventHandler<T> handler, T eventArgs) where T : EventArgs
        {
            if (handler == null) return;

            handler.Invoke(sender, eventArgs);
        }

        #endregion

        #region Constructors

        public MumbleConnection(string host, string username, int port = 64738)
        {
            this.host = host;
            this.port = port;
            this.username = username;

            connected = false;

            mumbleVersion = (1 << 16) + (2 << 8) + 3;
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            tcpClient = new TcpClient(host, port);

            sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate, null);

            sslStream.AuthenticateAsClient(host);

            connected = true;

#if DEBUG
            OnPacketReceived += PacketReceivedHandler;
#endif

            listenThread = new Thread(Listen);
            listenThread.Start();

            pingerThread = new Thread(Pinger);
            pingerThread.Start();
        }

        public void Disconnect()
        {
            connected = false;

#if DEBUG
            OnPacketReceived -= PacketReceivedHandler;
#endif
            try
            {
                sslStream.Close();
            }
            catch (Exception)
            {
                tcpClient.Close();
            }
        }

        #endregion

        #region Public Send Methods

        public void SendVersion(string clientVersion)
        {
            var message = new Version();

            message.release = clientVersion;
            message.version = mumbleVersion;
            message.os = Environment.OSVersion.Platform.ToString();
            message.os_version = Environment.OSVersion.VersionString;

            MumbleWrite(message);
        }


        public void SendAuthenticate()
        {
            var message = new Authenticate();

            message.username = username;
            message.celt_versions.Add(-2147483637);

            MumbleWrite(message);
        }

        public void SendUDPTunnel(byte[] packet)
        {
            var message = new UDPTunnel();

            message.packet = packet;

            MumbleWrite(message);
        }

        public void SendPing()
        {
            var message = new Ping();

            MumbleWrite(message);
        }

        public void SendReject()
        {
            var message = new Reject();

            MumbleWrite(message);
        }

        public void SendServerSync()
        {
            var message = new ServerSync();

            MumbleWrite(message);
        }

        public void SendChannelRemove()
        {
            var message = new ChannelRemove();

            MumbleWrite(message);
        }

        public void SendChannelState()
        {
            var message = new ChannelState();

            MumbleWrite(message);
        }

        public void SendUserRemove()
        {
            var message = new UserRemove();

            MumbleWrite(message);
        }

        public void SendUserState(MumbleChannel channel)
        {
            var message = new UserState();

            if (channel != null) { message.channel_id = channel.ID; }

            MumbleWrite(message);
        }

        public void SendBanList()
        {
            var message = new BanList();

            MumbleWrite(message);
        }

        public void SendTextMessage(string text, IEnumerable<MumbleChannel> channels, IEnumerable<MumbleChannel> trees, IEnumerable<MumbleUser> users)
        {
            var message = new TextMessage { message = text };

            if (channels != null) { message.channel_id.AddRange(channels.Select(channel => channel.ID)); }
            if (trees != null) { message.tree_id.AddRange(trees.Select(channel => channel.ID)); }
            if (users != null) { message.session.AddRange(users.Select(user => user.Session)); }

            MumbleWrite(message);
        }

        public void SendPermissionDenied()
        {
            var message = new PermissionDenied();

            MumbleWrite(message);
        }

        public void SendACL()
        {
            var message = new ACL();

            MumbleWrite(message);
        }

        public void SendQueryUsers()
        {
            var message = new QueryUsers();

            MumbleWrite(message);
        }

        public void SendCryptSetup()
        {
            var message = new CryptSetup();

            MumbleWrite(message);
        }

        public void SendContextActionModify()
        {
            var message = new ContextActionModify();

            MumbleWrite(message);
        }

        public void SendContextAction()
        {
            var message = new ContextAction();

            MumbleWrite(message);
        }

        public void SendUserList()
        {
            var message = new UserList();

            MumbleWrite(message);
        }

        public void SendVoiceTarget()
        {
            var message = new VoiceTarget();

            MumbleWrite(message);
        }

        public void SendPermissionQuery()
        {
            var message = new PermissionQuery();

            MumbleWrite(message);
        }

        public void SendCodecVersion()
        {
            var message = new CodecVersion();

            MumbleWrite(message);
        }

        public void SendUserStats()
        {
            var message = new UserStats();

            MumbleWrite(message);
        }

        public void SendRequestBlob()
        {
            var message = new RequestBlob();

            MumbleWrite(message);
        }

        public void SendServerConfig()
        {
            var message = new ServerConfig();

            MumbleWrite(message);
        }

        public void SendSuggestConfig()
        {
            var message = new SuggestConfig();

            MumbleWrite(message);
        }

        #endregion

        #region Private Methods

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void Listen()
        {
            while (connected)
            {
                var message = MumbleRead();
                if (message == null)
                {
                    break;
                }

                var temp = OnPacketReceived;
                if (temp != null)
                {
                    temp(this, new MumblePacketEventArgs(message));
                }
            }
            connected = false;
        }

        private void PacketReceivedHandler(object sender, MumblePacketEventArgs args)
        {
            var proto = args.Message as IProtocolHandler;

            Console.WriteLine(proto.Inspect());
        }


        private void Pinger()
        {
            while (connected)
            {
                Thread.Sleep(10000);
                SendPing();
            }
        }

        private void MumbleWrite(IExtensible message)
        {
            if (!connected) { return; }

            var sslStreamWriter = new BinaryWriter(sslStream);
            if (message is UDPTunnel)
            {
                var audioMessage = message as UDPTunnel;

                Int16 messageType = (Int16)MumbleProtocolFactory.MessageType(message);
                Int32 messageSize = (Int32)audioMessage.packet.Length;

                sslStreamWriter.Write(IPAddress.HostToNetworkOrder(messageType));
                sslStreamWriter.Write(IPAddress.HostToNetworkOrder(messageSize));
                sslStreamWriter.Write(audioMessage.packet);
            }
            else
            {
                MemoryStream messageStream = new MemoryStream();
                Serializer.NonGeneric.Serialize(messageStream, message);

                Int16 messageType = (Int16)MumbleProtocolFactory.MessageType(message);
                Int32 messageSize = (Int32)messageStream.Length;

                sslStreamWriter.Write(IPAddress.HostToNetworkOrder(messageType));
                sslStreamWriter.Write(IPAddress.HostToNetworkOrder(messageSize));
                messageStream.Position = 0;
                sslStreamWriter.Write(messageStream.ToArray());
            }
            sslStreamWriter.Flush();
        }

        private IExtensible MumbleRead()
        {
            var sslStreamReader = new BinaryReader(sslStream);

            IExtensible result;
            try
            {
                Int16 type = IPAddress.NetworkToHostOrder(sslStreamReader.ReadInt16());
                Int32 size = IPAddress.NetworkToHostOrder(sslStreamReader.ReadInt32());

                if (type == (int)MessageTypes.UDPTunnel)
                {
                    result = new UDPTunnel { packet = sslStreamReader.ReadBytes(size) };
                }
                else
                {
                    result = MumbleProtocolFactory.Deserialize((MessageTypes)type, size, sslStreamReader);
                }

            }
            catch (IOException exception)
            {
                result = null;
            }
            return result;
        }

        #endregion
    }
}
