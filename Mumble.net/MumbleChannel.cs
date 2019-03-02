using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Protocol.Mumble
{
    public class MumbleChannel
    {
        private MumbleClient _client;

        private readonly MumbleChannel _parentChannel;

        private readonly List<MumbleChannel> _subChannels = new List<MumbleChannel>();
        public ReadOnlyCollection<MumbleChannel> SubChannels => new ReadOnlyCollection<MumbleChannel>(_subChannels);

        private readonly List<MumbleUser> _users = new List<MumbleUser>();
        public ReadOnlyCollection<MumbleUser> Users => new ReadOnlyCollection<MumbleUser>(_users);

        public string Name { get; }
        public uint Id { get; }

        public MumbleChannel(MumbleClient client, ChannelState message)
        {
            _client = client;

            Id = message.channel_id;
            Name = message.name;

            client.Channels.Add(Id, this);
            client.Channels.TryGetValue(message.parent, out _parentChannel);

            if (IsRoot())
            {
                client.RootChannel = this;
            }
            else
            {
                _parentChannel?._subChannels.Add(this);
            }
        }

        public bool IsRoot()
        {
            return this == _parentChannel;
        }


        public void Update(ChannelState message)
        {

        }

        internal void AddLocalUser(MumbleUser user)
        {
            _users.Add(user);
        }

        internal void RemoveLocalUser(MumbleUser user)
        {
            _users.Remove(user);
        }

        public string Tree(int level = 0)
        {
            string result = $"{new String(' ', level)}C {Name} ({Id}){Environment.NewLine}";

            result = _subChannels.Aggregate(result, (current, channel) => current + channel.Tree(level + 1));

            return _users.Aggregate(result, (current, user) => current + user.Tree(level + 1));
        }
    }
}
