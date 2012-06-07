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
        public bool Deaf { get; private set; }
        public bool DeafSelf { get; private set; }
        public bool Mute { get; private set; }
        public bool MuteSelf { get; private set; }

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

            if (message.deafSpecified) { Deaf = message.deaf; }
            if (message.self_deafSpecified) { DeafSelf = message.self_deaf; }
            if (message.muteSpecified) { Mute = message.mute; }
            if (message.self_muteSpecified) { MuteSelf = message.self_mute; }
        }

        public void Update(MumbleProto.UserRemove message)
        {
            client.Channels.Remove(this.Session);
            Channel.RemoveLocalUser(this);
        }

        public string Tree(int level)
        {
            return new String(' ', level) + "U " + Name + " (" + Session + ")" + Environment.NewLine;
        }

    }
}
