using ArkUassetReader.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities
{
    public class UProperty
    {
        public string name;
        public int unknown1;
        public string type;
        public int unknown2;
        public int length;
        public int index;

        public UAssetFile source;

        public long fileLocation;

        public UProperty(IOMemoryStream ms, UAssetFile f, bool isArray, bool isStruct = false)
        {
            fileLocation = ms.position;
            if(!isArray)
            {
                name = ms.ReadNameTableEntry(f);
                unknown1 = ms.ReadInt();
                type = ms.ReadNameTableEntry(f);
                unknown2 = ms.ReadInt();
                length = ms.ReadInt();
                index = ms.ReadInt();
            }
            //ms.position += length;
        }

        public static UProperty ReadAnyProp(IOMemoryStream ms, UAssetFile f, out List<string> warnings, bool isArray = false, string arrayType = null, bool isStruct = false, bool quiet = false, bool readStructs = false)
        {
            long startPos = ms.position;
            warnings = new List<string>();

            //Check if this is none
            int name = -1;
            string name_string = null;
            if(!isArray)
            {
                name = ms.ReadInt();
                if (name < f.name_table.Length)
                {
                    name_string = f.name_table[name];
                    if (f.name_table[name] == "None")
                        return null; //End.
                }

            }

            //Read type
            if(!isArray)
                ms.position += 4;

            string type;
            if (!isArray)
                type = ms.ReadNameTableEntry(f);
            else
                type = arrayType;

            //Reset
            ms.position = startPos;

            //Switch
            UProperty u;
            switch(type)
            {
                case "ArrayProperty": u = new ArrayProperty(ms, f, isArray, readStructs:readStructs); break; 
                case "BoolProperty": u = new BoolProperty(ms, f, isArray); break;
                case "ByteProperty": u = new ByteProperty(ms, f, isArray); break; 
                case "DoubleProperty": u = new DoubleProperty(ms, f, isArray); break;
                case "FloatProperty": u = new FloatProperty(ms, f, isArray); break;
                case "Int16Property": u = new Int16Property(ms, f, isArray); break;
                case "Int8Property": u = new Int8Property(ms, f, isArray); break;
                case "IntProperty": u = new IntProperty(ms, f, isArray); break;
                case "NameProperty": u = new NameProperty(ms, f, isArray); break;
                case "ObjectProperty": u = new ObjectProperty(ms, f, isArray); break; 
                case "StrProperty": u = new StrProperty(ms, f, isArray); break;
                case "StructProperty": u = new StructProperty(ms, f, isArray, isStruct, skip:!readStructs); break; 
                case "TextProperty": u = new TextProperty(ms, f, isArray); break;
                case "UInt16Property": u = new UInt16Property(ms, f, isArray); break;
                case "UInt32Property": u = new UInt32Property(ms, f, isArray); break;
                case "UInt64Property": u = new UInt64Property(ms, f, isArray); break;
                default:
                    //Warn and continue reading. This will probably fail.
                    if(!quiet)
                        Console.WriteLine($"FAIL: Unknown type '{type}'. Name: {name_string}; This will likely cause a crash.");
                    throw new Exception();
                    return null;
            }
            u.source = f;
            return u;
        }
    }
}
