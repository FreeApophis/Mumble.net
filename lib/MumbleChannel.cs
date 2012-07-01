using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MumbleProto;

namespace Protocols.Mumble
{

    public class MumbleChannel
    {
        public string Name { get; private set; }
        private byte[] descriptionHash;
        public string Description { get; private set; }
        public uint ID { get; private set; }
        public bool Temporary { get; set; }
        public int Position { get; set; }

        public bool IsRoot { get { return this == parentChannel; } }

        private MumbleClient client;

        private MumbleChannel parentChannel;

        private readonly List<MumbleChannel> subChannels = new List<MumbleChannel>();
        public ReadOnlyCollection<MumbleChannel> SubChannels
        {
            get
            {
                return new ReadOnlyCollection<MumbleChannel>(subChannels);
            }
        }

        public IEnumerable<MumbleChannel> OrderedSubchannels
        {
            get
            {
                return subChannels.OrderBy(ch => ch.Position);
            }
        }

        private readonly List<MumbleUser> users = new List<MumbleUser>();
        public ReadOnlyCollection<MumbleUser> Users
        {
            get
            {
                return new ReadOnlyCollection<MumbleUser>(users);
            }
        }

        public MumbleChannel(MumbleClient client, ChannelState message)
        {
            this.client = client;

            ID = message.channel_id;
            Name = message.name;

            client.Channels.Add(ID, this);
            client.Channels.TryGetValue(message.parent, out parentChannel);

            if (IsRoot)
            {
                client.RootChannel = this;
            }
            else
            {
                parentChannel.subChannels.Add(this);
            }
        }



        internal void Update(ChannelState message)
        {
            if (message.temporarySpecified) { Temporary = message.temporary; }
            if (message.positionSpecified) { Position = message.position; }

            if (message.description_hashSpecified && descriptionHash != message.description_hash)
            {
                descriptionHash = message.description_hash;

                client.SendRequestBlob(Enumerable.Empty<UInt32>(), Enumerable.Empty<UInt32>(), Enumerable.Repeat(ID, 1));
            }
            if (message.descriptionSpecified) { Description = message.description; }
        }

        internal void Remove()
        {
            parentChannel.subChannels.Remove(this);
            client.Channels.Remove(ID);
        }

        internal void AddLocalUser(MumbleUser user)
        {
            users.Add(user);
        }

        internal void RemoveLocalUser(MumbleUser user)
        {
            users.Remove(user);
        }

        public string Tree(int level = 0)
        {
            string result = new String(' ', level) + "C " + Name + " (" + ID + ")" + Environment.NewLine;

            result = subChannels.Aggregate(result, (current, channel) => current + channel.Tree(level + 1));

            return users.Aggregate(result, (current, user) => current + user.Tree(level + 1));
        }
    }
}