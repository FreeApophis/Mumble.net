using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocols.Mumble
{
    public class MumbleUser
    {
        private MumbleClient client;

        public MumbleChannel Channel { get; private set; }
        public string Name { get; private set; }
        public uint Session { get; private set; }

        public MumbleUser(MumbleClient client, MumbleProto.UserState message)
        {
            this.client = client;
            Name = message.name;
            Session = message.session;

            client.Users.Add(Session, this);

            this.Channel = client.Channels[message.channel_id];

            this.Channel.AddLocalUser(this);
        }

        public void Update(MumbleProto.UserState message)
        {
            if (message.channel_idSpecified && message.channel_id != Channel.ID)
            {
                Channel.RemoveLocalUser(this);
                Channel = client.Channels[message.channel_id];
                Channel.AddLocalUser(this);
            }
        }

        public string Tree(int level)
        {
            return new String(' ', level) + "U " + Name + " (" + Session + ")" + Environment.NewLine;
        }

    }
}
