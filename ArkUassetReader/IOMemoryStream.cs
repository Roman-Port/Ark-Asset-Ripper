using ArkUassetReader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkUassetReader
{
    public class IOMemoryStream
    {
        public bool is_little_endian = true;

        public MemoryStream ms = new MemoryStream();

        public long position
        {
            get
            {
                return ms.Position;
            }
            set
            {
                ms.Position = value;
            }
        }

        public IOMemoryStream(MemoryStream ms, bool is_little_endian)
        {
            this.ms = ms;
            this.is_little_endian = is_little_endian;
        }

        //API deserialization
        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(PrivateReadBytes(2), 0);
        }

        public string ReadNameTableEntry(UAssetFile f)
        {
            int index = ReadInt();
            if (index < 0)
                return "NOT FOUND";
            return f.name_table[index];
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(PrivateReadBytes(2), 0);
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(PrivateReadBytes(4), 0);
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(PrivateReadBytes(4), 0);
        }

        public ulong ReadULong()
        {
            byte[] buf = PrivateReadBytes(8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public long ReadLong()
        {
            return BitConverter.ToInt64(PrivateReadBytes(4), 0);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(PrivateReadBytes(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(PrivateReadBytes(8), 0);
        }

        public bool ReadIntBool()
        {
            int data = ReadInt();
            //This is really bad, Wildcard....
            if (data != 0 && data != 1)
                throw new Exception("Expected boolean, got " + data);
            return data == 1;
        }

        public bool ReadByteBool()
        {
            return ReadByte() != 0x00;
        }

        public string ReadUEString(int maxLen = int.MaxValue)
        {
            //Read length
            int length = this.ReadInt();
            if (length == 0)
                return "";
            //Validate length
            if (length > maxLen)
                throw new Exception("Failed to read null-terminated string; Length from file exceeded maximum length requested.");
            if (length < 0)
                throw new Exception("Failed to read null-terminated string; Length was less than 0.");
            //Read this many bytes.
            byte[] buf = ReadBytes(length - 1);
            //Read null byte, but discard
            byte nullByte = ReadByte();
            if (nullByte != 0x00)
                throw new Exception("Failed to read null-terminated string; Terminator was not null!");
            //Convert to string
            return Encoding.UTF8.GetString(buf);
        }

        public byte[] ReadBytes(int length)
        {
            byte[] buf = new byte[length];
            ms.Read(buf, 0, length);
            return buf;
        }

        public byte ReadByte()
        {
            return ReadBytes(1)[0];
        }

        //Private deserialization API
        private byte[] PrivateReadBytes(int size)
        {
            //Read in from the buffer and respect the little endian setting.
            byte[] buf = new byte[size];
            //Read
            ms.Read(buf, 0, size);
            //Respect endians
            if (is_little_endian != BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            return buf;
        }
    }
}
