using System;
using System.Linq;

namespace Protocols.Mumble
{
    public class MumbleUser
    {
        private readonly MumbleClient client;

        public MumbleChannel Channel { get; private set; }
        public string Name { get; private set; }
        public UInt32 Session { get; private set; }
        public bool Deaf { get; private set; }
        public bool DeafSelf { get; private set; }
        public bool Mute { get; private set; }
        public bool MuteSelf { get; private set; }
        public bool Suppress { get; private set; }
        public string CertificateHash { get; private set; }
        public bool PrioritySpeaker { get; private set; }
        public bool Recording { get; private set; }
        public UInt32 UserID { get; private set; }
        private byte[] textureHash;
        public byte[] Texture { get; private set; }
        private byte[] commentHash;
        public string Comment { get; private set; }

        internal MumbleUser(MumbleClient client, MumbleProto.UserState message)
        {
            this.client = client;
            Name = message.name;
            Session = message.session;

            client.Users.Add(Session, this);

            Channel = client.Channels[message.channel_id];

            Channel.AddLocalUser(this);
        }

        internal void Update(MumbleProto.UserState message)
        {
            if (message.channel_idSpecified && message.channel_id != Channel.ID)
            {
                Channel.RemoveLocalUser(this);
                Channel = client.Channels[message.channel_id];
                Channel.AddLocalUser(this);
            }

            if (message.deafSpecified) { Deaf = message.deaf; }
            if (message.self_deafSpecified) { DeafSelf = message.self_deaf; }
            if (message.muteSpecified) { Mute = message.mute; }
            if (message.self_muteSpecified) { MuteSelf = message.self_mute; }
            if (message.suppressSpecified) { Suppress = message.suppress; }
            if (message.hashSpecified) { CertificateHash = message.hash; }
            if (message.priority_speakerSpecified) { PrioritySpeaker = message.priority_speaker; }
            if (message.recordingSpecified) { Recording = message.recording; }
            if (message.user_idSpecified) { UserID = message.user_id; }

            if (message.texture_hashSpecified && textureHash != message.texture_hash)
            {
                textureHash = message.texture_hash;

                client.SendRequestBlob(Enumerable.Repeat(Session, 1), Enumerable.Empty<UInt32>(), Enumerable.Empty<UInt32>());
            }
            if (message.textureSpecified) { Texture = message.texture; }

            if (message.comment_hashSpecified && commentHash != message.comment_hash)
            {
                commentHash = message.comment_hash;

                client.SendRequestBlob(Enumerable.Empty<UInt32>(), Enumerable.Repeat(Session, 1), Enumerable.Empty<UInt32>());
            }
            if (message.commentSpecified) { Comment = message.comment; }

        }

        internal void Update(MumbleProto.UserRemove message)
        {
            client.Channels.Remove(Session);
            Channel.RemoveLocalUser(this);
        }

        internal void Remove()
        {
            Channel.RemoveLocalUser(this);
            client.Users.Remove(Session);
        }

        public string Tree(int level)
        {
            return new String(' ', level) + "U " + Name + " (" + Session + ")" + Environment.NewLine;
        }
    }
}
