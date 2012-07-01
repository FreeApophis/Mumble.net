
namespace Protocols.Mumble
{
    class Permission
    {
        #region Local Permissions
        // Write access to channel control. Implies all other permissions (except Speak).
        public bool CanWritePermissions { get; private set; }
        // Traverse channel. Without this, a client cannot reach subchannels, no matter which privileges he has there.
        public bool CanTraverse { get; private set; }
        // Can enter channel.
        public bool CanEnter { get; private set; }
        // Can speek in channel.
        public bool CanSpeak { get; private set; }
        // Whisper to channel. This is different from Speak, so you can set up different permissions.
        public bool CanWhisper { get; private set; }
        // Mute and deafen other users in this channel.
        public bool CanMuteDeafen { get; private set; }
        // Move users from channel. You need this permission in both the source and destination channel to move anoth$
        public bool CanMove { get; private set; }
        // Make new channel as a subchannel of this channel.
        public bool CanMakeChannel { get; private set; }
        // Make new temporary channel as a subchannel of this channel.
        public bool CanMakeTempChannel { get; private set; }
        // Link this channel. You need this permission in both the source and destination channel to link channels, o$
        public bool CanLinkChannel { get; private set; }
        // Kick user from server. Only valid on root channel.
        public bool CanTextMessage { get; private set; }
        #endregion

        #region Global Permissions: Only valid on root channel.
        // Kick user from server.
        public bool CanKick { get; private set; }
        // Ban user from server.
        public bool CanBan { get; private set; }
        // Register and unregister other users.
        public bool CanRegister { get; private set; }
        // Register and unregister self.
        public bool CanRegisterSelf { get; private set; }
        #endregion

        // cached
        public bool Cached { get; private set; }

        Permission(uint permission)
        {
            CanWritePermissions = ((permission & 0x1) > 0) ? true : false;
            CanTraverse = ((permission & 0x2) > 0) ? true : false;
            CanEnter = ((permission & 0x4) > 0) ? true : false;
            CanSpeak = ((permission & 0x8) > 0) ? true : false;
            CanMuteDeafen = ((permission & 0x10) > 0) ? true : false;
            CanMove = ((permission & 0x20) > 0) ? true : false;
            CanMakeChannel = ((permission & 0x40) > 0) ? true : false;
            CanLinkChannel = ((permission & 0x80) > 0) ? true : false;
            CanWhisper = ((permission & 0x100) > 0) ? true : false;
            CanTextMessage = ((permission & 0x200) > 0) ? true : false;
            CanMakeTempChannel = ((permission & 0x400) > 0) ? true : false;

            CanKick = ((permission & 0x10000) > 0) ? true : false;
            CanBan = ((permission & 0x20000) > 0) ? true : false;
            CanRegister = ((permission & 0x40000) > 0) ? true : false;
            CanRegisterSelf = ((permission & 0x80000) > 0) ? true : false;

            Cached = ((permission & 0x8000000) > 0) ? true : false;
        }

        public uint ToInt()
        {
            uint bits = 0;

            if (CanWritePermissions) { bits |= 0x1; }
            if (CanTraverse) { bits |= 0x2; }
            if (CanEnter) { bits |= 0x4; }
            if (CanSpeak) { bits |= 0x8; }
            if (CanMuteDeafen) { bits |= 0x10; }
            if (CanMove) { bits |= 0x20; }
            if (CanMakeChannel) { bits |= 0x40; }
            if (CanLinkChannel) { bits |= 0x80; }
            if (CanWhisper) { bits |= 0x100; }
            if (CanTextMessage) { bits |= 0x200; }
            if (CanMakeTempChannel) { bits |= 0x400; }

            if (CanKick) { bits |= 0x10000; }
            if (CanBan) { bits |= 0x20000; }
            if (CanRegister) { bits |= 0x40000; }
            if (CanRegisterSelf) { bits |= 0x80000; }

            if (Cached) { bits |= 0x8000000; }

            return bits;
        }
    }
}
