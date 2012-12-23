using System;
using System.Collections.Generic;
using System.Linq;

namespace Protocol.Mumble
{
    public struct TypeTarget
    {
        public int Type;
        public int Target;
    }

    public class AudioPacket
    {
        private int index;
        private byte[] packet;
        private List<byte> encoderPacket;

        public byte[] Packet
        {
            get 
            {
                return decode ? packet : encoderPacket.ToArray();
            }
        }

        public byte[] Payload
        {
            get
            {
                return packet.Skip(index).ToArray();
            }

            set
            {
                encoderPacket.AddRange(value);
            }
        }


        private bool decode;

        public AudioPacket(byte[] packet)
        {
            this.packet = packet;
            decode = true;
        }

        public AudioPacket()
        {
            decode = false;
            encoderPacket = new List<byte>();
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
            encoderPacket.Add((byte)(value.Target << 5 | value.Type));
        }

        public void EncodeVarint(UInt64 value)
        {
            if (((value & 0x8000000000000000) != 0) && (~value < 0x100000000))
            {
                // Signed number.
                value = ~value;
                if (value <= 0x3)
                {
                    // Shortcase for -1 to -4
                    encoderPacket.Add((byte)(0xFC | value));
                    return;
                }
                encoderPacket.Add(0xF8);
            }
            if (value < 0x80)
            {
                // Need top bit clear
                encoderPacket.Add((byte)value);
            }
            if (value < 0x4000)
            {
                // Need top two bits clear
                encoderPacket.Add((byte)((value >> 8) | 0x80));
                encoderPacket.Add((byte)(value & 0xFF));
            }
            else if (value < 0x200000)
            {
                // Need top three bits clear
                encoderPacket.Add((byte)((value >> 16) | 0xC0));
                encoderPacket.Add((byte)((value >> 8) & 0xFF));
                encoderPacket.Add((byte)(value & 0xFF));
            }
            else if (value < 0x10000000)
            {
                // Need top four bits clear
                encoderPacket.Add((byte)((value >> 24) | 0xE0));
                encoderPacket.Add((byte)((value >> 16) & 0xFF));
                encoderPacket.Add((byte)((value >> 8) & 0xFF));
                encoderPacket.Add((byte)(value & 0xFF));
            }
            else if (value < 0x100000000)
            {
                // It's a full 32-bit integer.
                encoderPacket.Add(0xF0);
                encoderPacket.Add((byte)((value >> 24) & 0xFF));
                encoderPacket.Add((byte)((value >> 16) & 0xFF));
                encoderPacket.Add((byte)((value >> 8) & 0xFF));
                encoderPacket.Add((byte)(value & 0xFF));
            }
            else
            {
                // It's a 64-bit value.
                encoderPacket.Add(0xF4);
                encoderPacket.Add((byte)((value >> 56) & 0xFF));
                encoderPacket.Add((byte)((value >> 48) & 0xFF));
                encoderPacket.Add((byte)((value >> 40) & 0xFF));
                encoderPacket.Add((byte)((value >> 32) & 0xFF));
                encoderPacket.Add((byte)((value >> 24) & 0xFF));
                encoderPacket.Add((byte)((value >> 16) & 0xFF));
                encoderPacket.Add((byte)((value >> 8) & 0xFF));
                encoderPacket.Add((byte)(value & 0xFF));
            }
        }

        private UInt64 Next()
        {
            var result = packet[index];

            index++;

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
                        index++;
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
