using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;

namespace Protocol.Mumble
{
    class MumbleProtocolFactory
    {
        private static readonly Dictionary<MessageTypes, Type> Types = new Dictionary<MessageTypes, Type> 
        {
            { MessageTypes.Version, typeof(Version) },
            { MessageTypes.UDPTunnel, typeof(UDPTunnel) },
            { MessageTypes.Authenticate, typeof(Authenticate) },
            { MessageTypes.Ping, typeof(Ping) },
            { MessageTypes.Reject, typeof(Reject) },
            { MessageTypes.ServerSync, typeof(ServerSync) },
            { MessageTypes.ChannelRemove, typeof(ChannelRemove) },
            { MessageTypes.ChannelState, typeof(ChannelState) },
            { MessageTypes.UserRemove, typeof(UserRemove) },
            { MessageTypes.UserState, typeof(UserState) },
            { MessageTypes.BanList, typeof(BanList) },
            { MessageTypes.TextMessage, typeof(TextMessage) },
            { MessageTypes.PermissionDenied, typeof(PermissionDenied) },
            { MessageTypes.ACL, typeof(ACL) },
            { MessageTypes.QueryUsers, typeof(QueryUsers) },
            { MessageTypes.CryptSetup, typeof(CryptSetup) },
            { MessageTypes.ContextActionModify, typeof(ContextActionModify) },
            { MessageTypes.ContextAction, typeof(ContextAction) },
            { MessageTypes.UserList, typeof(UserList) },
            { MessageTypes.VoiceTarget, typeof(VoiceTarget) },
            { MessageTypes.PermissionQuery, typeof(PermissionQuery) },
            { MessageTypes.CodecVersion, typeof(CodecVersion) },
            { MessageTypes.UserStats, typeof(UserStats) },
            { MessageTypes.RequestBlob, typeof(RequestBlob) },
            { MessageTypes.ServerConfig, typeof(ServerConfig) },
            { MessageTypes.SuggestConfig, typeof(SuggestConfig) },
        };

        public static IExtensible Create(MessageTypes type)
        {
            var product = Types[type];

            return (IExtensible)Activator.CreateInstance(product);
        }

        public static IExtensible Deserialize(MessageTypes type, int size, BinaryReader stream)
        {
            var messageStream = new MemoryStream(stream.ReadBytes(size));

            return (IExtensible)Serializer.NonGeneric.Deserialize(Types[type], messageStream);
        }

        public static MessageTypes MessageType(IExtensible mumbleProto)
        {
            return Types.First(kvp => kvp.Value == mumbleProto.GetType()).Key;
        }
    }
}
