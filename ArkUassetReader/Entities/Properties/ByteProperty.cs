
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class ByteProperty : UProperty
    {
        public string enumName;

        public bool isNormalByte; //If this is true, this is just a normal byte. If it is not true, use the ClassName instead

        //=== VALUES ===
        public string enumValue; //Use ONLY if the above boolean is false
        public byte byteValue; //Use ONLY if the above boolean is true

        public ByteProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            if(length == 1)
            {


                //Read in the enum name
                enumName = ms.ReadNameTableEntry(f);

                //That can be None, but cannot be null.
                if (enumName == null)
                    throw new Exception("Tried to read enum type, but got null!");

                isNormalByte = enumName == "None";

                //If that type is a None, this is not an enum. If it is, this is an enum. Read the name.
                if (isNormalByte)
                {
                    byteValue = ms.ReadByte();
                    ms.ReadInt();
                }
                else
                    enumValue = ms.ReadNameTableEntry(f);
            } else if(length == 8)
            {
                //If the length is 8, this is an enum. It seems to follow like this...
                //Enum name
                //Int, usually 0
                //Enum value
                //Int, usually 0
                ms.position += 16;
            }
            else
            {
                Console.WriteLine($"Warning: Unknown ByteProperty length '{length}'. Skipping...");
                ms.position += length;
            }
        }
    }
}
