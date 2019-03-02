using System;
using System.Collections.Generic;
using System.Linq;

namespace Protocol.Mumble
{
    public class AudioPacket
    {
        private int _index;
        private readonly byte[] _packet;
        private readonly List<byte> _encoderPacket;

        public byte[] Packet => _decode ? _packet : _encoderPacket.ToArray();

        public byte[] Payload
        {
            get => _packet.Skip(_index).ToArray();

            set => _encoderPacket.AddRange(value);
        }


        private readonly bool _decode;

        public AudioPacket(byte[] packet)
        {
            _packet = packet;
            _decode = true;
        }

        public AudioPacket()
        {
            _decode = false;
            _encoderPacket = new List<byte>();
        }

        public TypeTarget DecodeTypeTarget()
        {
            byte head = (byte)Next();

            var target = (head & 0xE0) >> 5;
            var type = (head & 0x1F);

            return new TypeTarget { Target = target, Type = type };
        }

        public void EncodeTypeTarget(TypeTarget value)
        {
            _encoderPacket.Add((byte)(value.Target << 5 | value.Type));
        }

        public void EncodeVarint(UInt64 value)
        {
            if (((value & 0x8000000000000000) != 0) && (~value < 0x100000000))
            {
                // Signed number.
                value = ~value;
                if (value <= 0x3)
                {
                    // Short case for -1 to -4
                    _encoderPacket.Add((byte)(0xFC | value));
                    return;
                }
                _encoderPacket.Add(0xF8);
            }
            if (value < 0x80)
            {
                // Need top bit clear
                _encoderPacket.Add((byte)value);
            }
            if (value < 0x4000)
            {
                // Need top two bits clear
                _encoderPacket.Add((byte)((value >> 8) | 0x80));
                _encoderPacket.Add((byte)(value & 0xFF));
            }
            else if (value < 0x200000)
            {
                // Need top three bits clear
                _encoderPacket.Add((byte)((value >> 16) | 0xC0));
                _encoderPacket.Add((byte)((value >> 8) & 0xFF));
                _encoderPacket.Add((byte)(value & 0xFF));
            }
            else if (value < 0x10000000)
            {
                // Need top four bits clear
                _encoderPacket.Add((byte)((value >> 24) | 0xE0));
                _encoderPacket.Add((byte)((value >> 16) & 0xFF));
                _encoderPacket.Add((byte)((value >> 8) & 0xFF));
                _encoderPacket.Add((byte)(value & 0xFF));
            }
            else if (value < 0x100000000)
            {
                // It's a full 32-bit integer.
                _encoderPacket.Add(0xF0);
                _encoderPacket.Add((byte)((value >> 24) & 0xFF));
                _encoderPacket.Add((byte)((value >> 16) & 0xFF));
                _encoderPacket.Add((byte)((value >> 8) & 0xFF));
                _encoderPacket.Add((byte)(value & 0xFF));
            }
            else
            {
                // It's a 64-bit value.
                _encoderPacket.Add(0xF4);
                _encoderPacket.Add((byte)((value >> 56) & 0xFF));
                _encoderPacket.Add((byte)((value >> 48) & 0xFF));
                _encoderPacket.Add((byte)((value >> 40) & 0xFF));
                _encoderPacket.Add((byte)((value >> 32) & 0xFF));
                _encoderPacket.Add((byte)((value >> 24) & 0xFF));
                _encoderPacket.Add((byte)((value >> 16) & 0xFF));
                _encoderPacket.Add((byte)((value >> 8) & 0xFF));
                _encoderPacket.Add((byte)(value & 0xFF));
            }
        }

        private UInt64 Next()
        {
            var result = _packet[_index];

            _index++;

            return result;
        }

        public UInt64 DecodeVarint()
        {
            UInt64 result = 0;

            UInt64 head = Next();

            if ((head & 0x80) == 0x00)
            {
                result = (head & 0x7F);
            }
            else if ((head & 0xC0) == 0x80)
            {
                result = (head & 0x3F) << 8 | Next();
            }
            else if ((head & 0xF0) == 0xF0)
            {
                switch (head & 0xFC)
                {
                    case 0xF0:
                        result = Next() << 24 | Next() << 16 | Next() << 8 | Next();
                        break;
                    case 0xF4:
                        result = Next() << 56 | Next() << 48 | Next() << 40 | Next() << 32 | Next() << 24 | Next() << 16 | Next() << 8 | Next();
                        break;
                    case 0xF8:
                        _index++;
                        result = DecodeVarint();
                        result = ~result;
                        break;
                    case 0xFC:
                        result = head & 0x03;
                        result = ~result;
                        break;
                    default:
                        throw new Exception("Decode Varint failed");
                }
            }
            else if ((head & 0xF0) == 0xE0)
            {
                result = (head & 0x0F) << 24 | Next() << 16 | Next() << 8 | Next();
            }
            else if ((head & 0xE0) == 0xC0)
            {
                result = (head & 0x1F) << 16 | Next() << 8 | Next();
            }

            return result;
        }
    }
}
