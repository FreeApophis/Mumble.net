using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Protocol.Mumble
{

    public class MumbleChannel
    {
        private MumbleClient client;

        private MumbleChannel parentChannel;

        private List<MumbleChannel> subChannels = new List<MumbleChannel>();
        public ReadOnlyCollection<MumbleChannel> SubChannels
        {
            get
            {
                return new ReadOnlyCollection<MumbleChannel>(subChannels);
            }
        }

        private List<MumbleUser> users = new List<MumbleUser>();
        public ReadOnlyCollection<MumbleUser> Users
        {
            get
            {
                return new ReadOnlyCollection<MumbleUser>(users);
            }
        }

        public string Name { get; private set; }
        public uint ID { get; private set; }

        public MumbleChannel(MumbleClient client, ChannelState message)
        {
            this.client = client;

            ID = message.channel_id;
            Name = message.name;

            client.Channels.Add(ID, this);
            client.Channels.TryGetValue(message.parent, out parentChannel);

            if (IsRoot())
            {
                client.RootChannel = this;
            }
            else
            {
                parentChannel.subChannels.Add(this);
            }
        }

        public bool IsRoot()
        {
            return this == parentChannel;
        }


        public void Update(ChannelState message)
        {

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

            foreach (var channel in subChannels)
            {
                result += channel.Tree(level + 1);
            }

            foreach (var user in users)
            {
                result += user.Tree(level + 1);
            }

            return result;
        }
    }
}
