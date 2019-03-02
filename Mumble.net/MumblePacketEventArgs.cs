using System;
using ProtoBuf;

namespace Protocol.Mumble
{
    public class MumblePacketEventArgs : EventArgs
    {
        public IExtensible Message { get; }

        public MumblePacketEventArgs(IExtensible message)
        {
            Message = message;
        }
    }
}
