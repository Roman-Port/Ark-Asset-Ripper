/*using ArkUassetReader.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities
{
    public class DotArkArray
    {
        public static UProperty ReadArray(IOMemoryStream ms, string[] name_table)
        {
            //Read all of the usual property stuff
            string name = ms.ReadNameTableEntry(name_table);
            int unknown1 = ms.ReadInt();
            string type = ms.ReadNameTableEntry(name_table);
            int unknown2 = ms.ReadInt();
            int length = ms.ReadInt();
            int unknown4 = ms.ReadInt();

            //First, read the type of the array.
            string arrayType = ms.ReadNameTableEntry(name_table);

            //Read through each of the values in the array.
            //ms.ms.Position += length;

            //Switch depending on the type
            UProperty r;
            switch (arrayType)
            {
                case "ObjectProperty": r = ReadObjectProperty(ms, name_table, length, arrayType); break;
                case "StructProperty": r = ReadStructProperty(ms, name_table, length, arrayType); break; //TODO
                case "UInt32Property": r = ReadUInt32Property(ms, name_table, length, arrayType); break;
                case "IntProperty": r = ReadIntProperty(ms, name_table, length, arrayType); break;
                case "UInt16Property": r = ReadUInt16Property(ms, name_table, length, arrayType); break;
                case "Int16Property": r = ReadInt16Property(ms, name_table, length, arrayType); break;
                case "ByteProperty": r = ReadByteProperty(ms, name_table, length, arrayType); break;
                case "Int8Property": r = ReadInt8Property(ms, name_table, length, arrayType); break;
                case "StrProperty": r = ReadStrProperty(ms, name_table, length, arrayType); break;
                case "UInt64Property": r = ReadUInt64Property(ms, name_table, length, arrayType); break;
                case "BoolProperty": r = ReadBoolProperty(ms, name_table, length, arrayType); break;
                case "FloatProperty": r = ReadFloatProperty(ms, name_table, length, arrayType); break;
                case "DoubleProperty": r = ReadDoubleProperty(ms, name_table, length, arrayType); break;
                case "NameProperty": r = ReadNameProperty(ms, name_table, length, arrayType); break;
                default:
                    throw new Exception($"Unknown ARK array type '{arrayType}'.");
            }

            return r;
        }

        private static UProperty ReadObjectProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<ObjectProperty> data = new List<ObjectProperty>();

            for (int i = 0; i < arraySize; i++)
            {
                data.Add(new ObjectProperty(ms, name_table));
            }

            //Create
            return new ArrayProperty<ObjectProperty>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        //TODO
        private static UProperty ReadStructProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            List<byte> data = new List<byte>();

            ms.ms.Position += length;

            //Create
            return new ArrayProperty<byte>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadUInt32Property(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<UInt32> data = new List<UInt32>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadUInt());

            //Create
            return new ArrayProperty<UInt32>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadIntProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<int> data = new List<int>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadInt());

            //Create
            return new ArrayProperty<int>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadUInt16Property(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<UInt16> data = new List<UInt16>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadUShort());

            //Create
            return new ArrayProperty<UInt16>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadInt16Property(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<Int16> data = new List<Int16>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadShort());

            //Create
            return new ArrayProperty<Int16>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadByteProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<byte> data = new List<byte>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadByte());

            //Create
            return new ArrayProperty<byte>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadInt8Property(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<byte> data = new List<byte>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadByte());

            //Create
            return new ArrayProperty<byte>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadStrProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<string> data = new List<string>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadUEString());

            //Create
            return new ArrayProperty<string>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadUInt64Property(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<UInt64> data = new List<UInt64>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadULong());

            //Create
            return new ArrayProperty<UInt64>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadBoolProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<bool> data = new List<bool>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadByteBool());

            //Create
            return new ArrayProperty<bool>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadFloatProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<float> data = new List<float>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadFloat());

            //Create
            return new ArrayProperty<float>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadDoubleProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<double> data = new List<double>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.ReadDouble());

            //Create
            return new ArrayProperty<double>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

        private static UProperty ReadNameProperty(IOMemoryStream ms, string[] name_table, int index, int length, string type)
        {
            //Open
            int arraySize = ms.ReadInt();

            List<string> data = new List<string>();

            for (int i = 0; i < arraySize; i++)
                data.Add(ms.Readstring(d));

            //Create
            return new ArrayProperty<string>
            {
                arrayType = type,
                index = index,
                items = data,
                size = length
            };
        }

    }
}
*/