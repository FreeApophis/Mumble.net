using System;
using ProtoBuf;

namespace Protocol.Mumble
{
    public class MumblePacketEventArgs : EventArgs
    {
        public IExtensible Message { get; private set; }

        public MumblePacketEventArgs(IExtensible message)
        {
            Message = message;
        }
    }
}
