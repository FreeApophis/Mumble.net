using System;
using System.Linq;
using System.Reflection;
using Protocols.Mumble;

namespace MumbleProto
{

    public interface IProtocolHandler
    {
        void HandleMessage(MumbleClient client);
    }

    public static class ProtocolHandler
    {
        public static string Inspect(this IProtocolHandler message)
        {
            if (message == null) { return "null"; }

            var type = message.GetType();
            string result = type.FullName + Environment.NewLine;
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (!property.IsDefined(typeof(ProtoBuf.ProtoMemberAttribute), false)) { continue; }

                PropertyInfo prop = property;
                var specified = properties.Where(p => p.Name == prop.Name + "Specified").FirstOrDefault();
                bool? hasField = null;
                if (specified != null)
                {
                    hasField = specified.GetValue(message, null) as bool?;
                }
                result += " " + property.Name + ": " + FormatValue(hasField, property.GetValue(message, null)) + Environment.NewLine;
            }

            return result;
        }

        private static string FormatValue(bool? info, object data)
        {
            switch (info)
            {
                case true:
                    return data.ToString();
                case false:
                    return "[EMPTY]";
                default:
                    return data.ToString();
            }
        }
    }




    public partial class Version : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {
            client.Update(this);
        }
    }

    public partial class UDPTunnel : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {
            var audioIn = new AudioPacket(packet);

            var type = audioIn.DecodeTypeTarget();
            var session = audioIn.DecodeVarint();
            var sequence = audioIn.DecodeVarint();

            Console.WriteLine(sequence);

            var audioOut = new AudioPacket();

            type.Target = 0;

            audioOut.EncodeTypeTarget(type);
            //audioOut.EncodeVarint(client.ClientUser.Session);
            audioOut.EncodeVarint(sequence + 5000);
            audioOut.Payload = audioIn.Payload;

            client.SendUDPTunnel(audioOut.Packet);
        }
    }

    public partial class Authenticate : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class Ping : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class Reject : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class ServerSync : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {
            client.Update(this);
        }
    }

    public partial class ChannelRemove : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class ChannelState : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {
            MumbleChannel channel;
            if (!client.Channels.TryGetValue(channel_id, out channel))
            {
                channel = new MumbleChannel(client, this);
            }
            channel.Update(this);

        }
    }

    public partial class UserRemove : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {
            MumbleUser user;
            if (client.Users.TryGetValue(session, out user))
            {
                user.Update(this);
            }


        }
    }

    public partial class UserState : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {
            MumbleUser user;
            if (!client.Users.TryGetValue(session, out user))
            {
                user = new MumbleUser(client, this);
            }
            user.Update(this);
        }
    }

    public partial class BanList : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class TextMessage : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class PermissionDenied : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class ACL : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class QueryUsers : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class CryptSetup : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class ContextActionModify : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class ContextAction : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class UserList : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class VoiceTarget : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class PermissionQuery : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class CodecVersion : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class UserStats : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class RequestBlob : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class ServerConfig : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

    public partial class SuggestConfig : IProtocolHandler
    {
        public void HandleMessage(MumbleClient client)
        {

        }
    }

}
