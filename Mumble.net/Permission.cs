namespace Protocol.Mumble
{
    class Permission
    {
        #region Local Permissions
        // Write access to channel control. Implies all other permissions (except Speak).
        public bool CanWritePermissions { get; }
        // Traverse channel. Without this, a client cannot reach sub-channels, no matter which privileges he has there.
        public bool CanTraverse { get; }
        // Can enter channel.
        public bool CanEnter { get; }
        // Can speak in channel.
        public bool CanSpeak { get; }
        // Whisper to channel. This is different from Speak, so you can set up different permissions.
        public bool CanWhisper { get; }
        // Mute and deafen other users in this channel.
        public bool CanMuteDeafen { get; }
        // Move users from channel. You need this permission in both the source and destination channel to move another channel
        public bool CanMove { get; }
        // Make new channel as a sub-channel of this channel.
        public bool CanMakeChannel { get; }
        // Make new temporary channel as a sub-channel of this channel.
        public bool CanMakeTempChannel { get; }
        // Link this channel. You need this permission in both the source and destination channel to link channels, ...
        public bool CanLinkChannel { get; }
        // Kick user from server. Only valid on root channel.
        public bool CanTextMessage { get; }
        #endregion Local Permissions

        #region Global Permissions: Only valid on root channel.
        // Kick user from server.
        public bool CanKick { get; }
        // Ban user from server.
        public bool CanBan { get; }
        // Register and un-register other users.
        public bool CanRegister { get; }
        // Register and ubn.register self.
        public bool CanRegisterSelf { get; }
        #endregion Global Permissions: Only valid on root channel.

        // cached
        public bool Cached { get; }

        Permission(uint permission)
        {
            CanWritePermissions = (permission & 0x1) > 0;
            CanTraverse = (permission & 0x2) > 0;
            CanEnter = (permission & 0x4) > 0;
            CanSpeak = (permission & 0x8) > 0;
            CanMuteDeafen = (permission & 0x10) > 0;
            CanMove = (permission & 0x20) > 0;
            CanMakeChannel = (permission & 0x40) > 0;
            CanLinkChannel = (permission & 0x80) > 0;
            CanWhisper = (permission & 0x100) > 0;
            CanTextMessage = (permission & 0x200) > 0;
            CanMakeTempChannel = (permission & 0x400) > 0;

            CanKick = (permission & 0x10000) > 0;
            CanBan = (permission & 0x20000) > 0;
            CanRegister = (permission & 0x40000) > 0;
            CanRegisterSelf = (permission & 0x80000) > 0;

            Cached = (permission & 0x8000000) > 0;
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
