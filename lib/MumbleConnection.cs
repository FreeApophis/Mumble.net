using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using MumbleProto;
using ProtoBuf;

namespace Protocols.Mumble
{


    public class MumbleConnection
    {
        private readonly int port;
        private readonly string host;
        private bool connected;

        private readonly uint mumbleVersion;
        private readonly string username;

        private TcpClient tcpClient;
        private SslStream sslStream;

        private Thread listenThread;
        private Thread pingerThread;

        #region Public events

        public event EventHandler<MumblePacketEventArgs> PacketReceivedEvent;

        #endregion

        #region Constructors

        public MumbleConnection()
        {
            port = 64738;

            host = "apophis.ch";
            username = "LeChuck";

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
            PacketReceivedEvent += OnPacketEvent;
#endif

            listenThread = new Thread(Listen);
            listenThread.Start();

            pingerThread = new Thread(Pinger);
            pingerThread.Start();
        }

        #endregion

        #region Public Send Methods

        public void SendVersion(string clientVersion)
        {
            var message = new MumbleProto.Version
            {
                release = clientVersion,
                version = mumbleVersion,
                os = Environment.OSVersion.Platform.ToString(),
                os_version = Environment.OSVersion.VersionString
            };

            MumbleWrite(message);
        }


        public void SendAuthenticate()
        {
            var message = new Authenticate { username = username };

            message.celt_versions.Add(-2147483637);

            MumbleWrite(message);
        }

        public void SendUDPTunnel(byte[] packet)
        {
            var message = new UDPTunnel { packet = packet };

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

        public void SendUserState()
        {
            var message = new UserState();

            MumbleWrite(message);
        }

        public void SendBanList()
        {
            var message = new BanList();

            MumbleWrite(message);
        }

        public void SendTextMessage()
        {
            var message = new TextMessage();

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

        public void SendRequestBlob(IEnumerable<UInt32> sessionTexture, IEnumerable<UInt32> sessionComment, IEnumerable<UInt32> channelDescription)
        {
            var message = new RequestBlob();

            message.session_texture.AddRange(sessionTexture);
            message.session_comment.AddRange(sessionComment);
            message.channel_description.AddRange(channelDescription);

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

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

                var temp = PacketReceivedEvent;
                if (temp != null)
                {
                    temp(this, new MumblePacketEventArgs(message));
                }
            }
            connected = false;
        }

        private static void OnPacketEvent(object sender, MumblePacketEventArgs args)
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
            var sslStreamWriter = new BinaryWriter(sslStream);
            if (message is UDPTunnel)
            {
                var audioMessage = message as UDPTunnel;

                var messageType = (Int16)MumbleProtocolFactory.MessageType(message);
                var messageSize = (Int32)audioMessage.packet.Length;

                sslStreamWriter.Write(IPAddress.HostToNetworkOrder(messageType));
                sslStreamWriter.Write(IPAddress.HostToNetworkOrder(messageSize));
                sslStreamWriter.Write(audioMessage.packet);
            }
            else
            {
                var messageStream = new MemoryStream();
                Serializer.NonGeneric.Serialize(messageStream, message);

                var messageType = (Int16)MumbleProtocolFactory.MessageType(message);
                var messageSize = (Int32)messageStream.Length;

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

            Int16 type = IPAddress.NetworkToHostOrder(sslStreamReader.ReadInt16());
            Int32 size = IPAddress.NetworkToHostOrder(sslStreamReader.ReadInt32());

            IExtensible result = type == (int)MessageTypes.UDPTunnel ? new UDPTunnel { packet = sslStreamReader.ReadBytes(size) } : MumbleProtocolFactory.Deserialize((MessageTypes)type, size, sslStreamReader);
            return result;
        }

        #endregion
    }
}
