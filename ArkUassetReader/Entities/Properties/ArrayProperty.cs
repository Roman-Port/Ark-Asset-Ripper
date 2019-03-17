
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class ArrayProperty : UProperty
    {
        public string arrayType;
        public List<UProperty> items;

        public ArrayProperty(IOMemoryStream ms, UAssetFile f, bool isArray, bool readStructs) : base(ms, f, isArray)
        {
            long startPos = ms.position;
            
            //Read classname
            arrayType = ms.ReadNameTableEntry(f);

            //Skip unknown int
            int uk1 = ms.ReadInt();

            //Get ending byte
            long end = ms.position + length - 0;

            //Skip this if we should skip StructProperties and this is a struct
            if (!readStructs && arrayType == "StructProperty")
            {
                ms.position = end;
                return;
            }

            //Read until end
            items = new List<UProperty>();
            while(true)
            {
                /*if(items.Count == 1)
                {
                    if(((StructProperty)items[0]).props.Count > 0)
                    {
                        if(((StructProperty)items[0]).props[0].name == "HairStyleNameString_8_59FC1375448902F8752BF083E928518D")
                        {
                            Console.Write("");
                        }
                    }
                }*/
                //Console.WriteLine(ms.position);
                if(arrayType == "StructProperty" || items.Count == 0)
                    ms.ReadInt();
                var p = UProperty.ReadAnyProp(ms, f, out List<string> warnings, true, arrayType, isStruct:true, readStructs:readStructs);
                
                items.Add(p);
                if (items.Count > 10000)
                    Console.WriteLine("tifu");
                if (ms.position >= end)
                    break;
            }

            //Ensure we're actually at the end
            ms.position = end;
        }
    }
}
