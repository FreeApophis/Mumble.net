using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.IO;

namespace Protocols.Mumble
{
    class MumbleProtocolFactory
    {
        private static Dictionary<MessageTypes, Type> types = new Dictionary<MessageTypes, Type> 
        {
            { MessageTypes.Version, typeof(MumbleProto.Version) },
            { MessageTypes.UDPTunnel, typeof(MumbleProto.UDPTunnel) },
            { MessageTypes.Authenticate, typeof(MumbleProto.Authenticate) },
            { MessageTypes.Ping, typeof(MumbleProto.Ping) },
            { MessageTypes.Reject, typeof(MumbleProto.Reject) },
            { MessageTypes.ServerSync, typeof(MumbleProto.ServerSync) },
            { MessageTypes.ChannelRemove, typeof(MumbleProto.ChannelRemove) },
            { MessageTypes.ChannelState, typeof(MumbleProto.ChannelState) },
            { MessageTypes.UserRemove, typeof(MumbleProto.UserRemove) },
            { MessageTypes.UserState, typeof(MumbleProto.UserState) },
            { MessageTypes.BanList, typeof(MumbleProto.BanList) },
            { MessageTypes.TextMessage, typeof(MumbleProto.TextMessage) },
            { MessageTypes.PermissionDenied, typeof(MumbleProto.PermissionDenied) },
            { MessageTypes.ACL, typeof(MumbleProto.ACL) },
            { MessageTypes.QueryUsers, typeof(MumbleProto.QueryUsers) },
            { MessageTypes.CryptSetup, typeof(MumbleProto.CryptSetup) },
            { MessageTypes.ContextActionModify, typeof(MumbleProto.ContextActionModify) },
            { MessageTypes.ContextAction, typeof(MumbleProto.ContextAction) },
            { MessageTypes.UserList, typeof(MumbleProto.UserList) },
            { MessageTypes.VoiceTarget, typeof(MumbleProto.VoiceTarget) },
            { MessageTypes.PermissionQuery, typeof(MumbleProto.PermissionQuery) },
            { MessageTypes.CodecVersion, typeof(MumbleProto.CodecVersion) },
            { MessageTypes.UserStats, typeof(MumbleProto.UserStats) },
            { MessageTypes.RequestBlob, typeof(MumbleProto.RequestBlob) },
            { MessageTypes.ServerConfig, typeof(MumbleProto.ServerConfig) },
            { MessageTypes.SuggestConfig, typeof(MumbleProto.SuggestConfig) },
        };

        public static IExtensible Create(MessageTypes type)
        {
            var product = types[type];

            return (IExtensible)Activator.CreateInstance(product);
        }

        public static IExtensible Deserialize(MessageTypes type, int size, BinaryReader stream)
        {
            var messageStream = new MemoryStream(stream.ReadBytes(size));

            return (IExtensible)Serializer.NonGeneric.Deserialize(types[type], messageStream);
        }

        public static MessageTypes MessageType(IExtensible mumbleProto)
        {
            return types.Where(kvp => kvp.Value == mumbleProto.GetType()).First().Key;
        }
    }
}
