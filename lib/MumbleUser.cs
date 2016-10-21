using System;

namespace Protocol.Mumble
{
    public class MumbleUser
    {
        private readonly MumbleClient _client;

        public MumbleChannel Channel { get; private set; }
        public string Name { get; }
        public uint Session { get; }
        public bool Deaf { get; private set; }
        public bool DeafSelf { get; private set; }
        public bool Mute { get; private set; }
        public bool MuteSelf { get; private set; }

        public MumbleUser(MumbleClient client, UserState message)
        {
            _client = client;
            Name = message.name;
            Session = message.session;

            client.Users.Add(Session, this);

            Channel = client.Channels[message.channel_id];

            Channel.AddLocalUser(this);
        }

        public void Update(UserState message)
        {
            if (message.channel_idSpecified && message.channel_id != Channel.ID)
            {
                Channel.RemoveLocalUser(this);
                Channel = _client.Channels[message.channel_id];
                Channel.AddLocalUser(this);
            }

            if (message.deafSpecified) { Deaf = message.deaf; }
            if (message.self_deafSpecified) { DeafSelf = message.self_deaf; }
            if (message.muteSpecified) { Mute = message.mute; }
            if (message.self_muteSpecified) { MuteSelf = message.self_mute; }
        }

        public void Update(UserRemove message)
        {
            _client.Channels.Remove(Session);
            Channel.RemoveLocalUser(this);
        }

        public string Tree(int level)
        {
            return $"{new string(' ', level)}U {Name} ({Session}){Environment.NewLine}";
        }

    }
}
