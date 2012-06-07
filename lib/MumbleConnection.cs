using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

using ProtoBuf;
using MumbleProto;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace Protocols.Mumble
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
            var message = new MumbleProto.Version();

            message.release = clientVersion;
            message.version = mumbleVersion;
            message.os = Environment.OSVersion.Platform.ToString();
            message.os_version = Environment.OSVersion.VersionString;

            MumbleWrite(message);
        }


        public void SendAuthenticate()
        {
            var message = new MumbleProto.Authenticate();

            message.username = username;
            message.celt_versions.Add(-2147483637);

            MumbleWrite(message);
        }

        public void SendUDPTunnel(byte[] packet)
        {
            var message = new MumbleProto.UDPTunnel();

            message.packet = packet;

            MumbleWrite(message);
        }

        public void SendPing()
        {
            var message = new MumbleProto.Ping();

            MumbleWrite(message);
        }

        public void SendReject()
        {
            var message = new MumbleProto.Reject();

            MumbleWrite(message);
        }

        public void SendServerSync()
        {
            var message = new MumbleProto.ServerSync();

            MumbleWrite(message);
        }

        public void SendChannelRemove()
        {
            var message = new MumbleProto.ChannelRemove();

            MumbleWrite(message);
        }

        public void SendChannelState()
        {
            var message = new MumbleProto.ChannelState();

            MumbleWrite(message);
        }

        public void SendUserRemove()
        {
            var message = new MumbleProto.UserRemove();

            MumbleWrite(message);
        }

        public void SendUserState()
        {
            var message = new MumbleProto.UserState();

            MumbleWrite(message);
        }

        public void SendBanList()
        {
            var message = new MumbleProto.BanList();

            MumbleWrite(message);
        }

        public void SendTextMessage()
        {
            var message = new MumbleProto.TextMessage();

            MumbleWrite(message);
        }

        public void SendPermissionDenied()
        {
            var message = new MumbleProto.PermissionDenied();

            MumbleWrite(message);
        }

        public void SendACL()
        {
            var message = new MumbleProto.ACL();

            MumbleWrite(message);
        }

        public void SendQueryUsers()
        {
            var message = new MumbleProto.QueryUsers();

            MumbleWrite(message);
        }

        public void SendCryptSetup()
        {
            var message = new MumbleProto.CryptSetup();

            MumbleWrite(message);
        }

        public void SendContextActionModify()
        {
            var message = new MumbleProto.ContextActionModify();

            MumbleWrite(message);
        }

        public void SendContextAction()
        {
            var message = new MumbleProto.ContextAction();

            MumbleWrite(message);
        }

        public void SendUserList()
        {
            var message = new MumbleProto.UserList();

            MumbleWrite(message);
        }

        public void SendVoiceTarget()
        {
            var message = new MumbleProto.VoiceTarget();

            MumbleWrite(message);
        }

        public void SendPermissionQuery()
        {
            var message = new MumbleProto.PermissionQuery();

            MumbleWrite(message);
        }

        public void SendCodecVersion()
        {
            var message = new MumbleProto.CodecVersion();

            MumbleWrite(message);
        }

        public void SendUserStats()
        {
            var message = new MumbleProto.UserStats();

            MumbleWrite(message);
        }

        public void SendRequestBlob()
        {
            var message = new MumbleProto.RequestBlob();

            MumbleWrite(message);
        }

        public void SendServerConfig()
        {
            var message = new MumbleProto.ServerConfig();

            MumbleWrite(message);
        }

        public void SendSuggestConfig()
        {
            var message = new MumbleProto.SuggestConfig();

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

                var temp = PacketReceivedEvent;
                if (temp != null)
                {
                    temp(this, new MumblePacketEventArgs(message));
                }
            }
            connected = false;
        }

        private void OnPacketEvent(object sender, MumblePacketEventArgs args)
        {
            var proto = args.Message as IProtocolHandler;

            System.Console.WriteLine(proto.Inspect());
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

            IExtensible result = null;

            Int16 type = IPAddress.NetworkToHostOrder(sslStreamReader.ReadInt16());
            Int32 size = IPAddress.NetworkToHostOrder(sslStreamReader.ReadInt32());

            if (type == (int)MessageTypes.UDPTunnel)
            {
                result = new UDPTunnel() { packet = sslStreamReader.ReadBytes(size) };
            }
            else
            {
                result = MumbleProtocolFactory.Deserialize((MessageTypes)type, size, sslStreamReader);
            }
            return result;
        }

        #endregion
    }
}
