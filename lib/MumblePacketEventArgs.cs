using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Protocols.Mumble
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
