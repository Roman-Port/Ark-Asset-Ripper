using ArkUassetReader.Entities.Properties.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class StructProperty : UProperty
    {
        public List<UProperty> props;
        public UStructData structData;
        public StructPropertyType structType;

        public StructProperty(IOMemoryStream ms, UAssetFile f, bool isArray, bool isStruct, bool skip) : base(ms, f, isArray, isStruct)
        {          
            //Read props until the next value is None.
            long startPos = ms.position;
            long endPos = startPos + length + 8;

            //Skip is possible right now because this is really buggy.
            if (skip)
            {
                ms.position = endPos;
                structType = StructPropertyType.skipped;
                return;
            }

            //Read a name table entry. If this matches the special, predefined, types, read them. Else, read this as a struct property list
            string typeName = "";
            int u3 = 0;

            
            try
            {
                typeName = ms.ReadNameTableEntry(f);
                u3 = ms.ReadInt();
                if (typeName == "LinearColor" || typeName == "Color" || typeName == "Quat" || typeName == "Vector2D" || typeName == "Vector" || typeName == "Rotator" || typeName == "Guid" )
                {
                    //Predefined
                    structType = StructPropertyType.predefined;
                    if(isArray)
                    {
                        //Skipping will not work
                        throw new Exception("Cannot skip predefined structs in arrays because no length is offered in arrays.");
                    } else
                    {
                        Console.WriteLine("Prefefined. Attempting to skip...");
                        ms.position = endPos;
                    }
                }
                else
                {
                    //Read as list of props
                    ms.position = startPos;

                    props = new List<UProperty>();
                    UProperty u = UProperty.ReadAnyProp(ms, f, out List<string> warnings, quiet:true);
                    while (u != null)
                    {
                        props.Add(u);
                        u = UProperty.ReadAnyProp(ms, f, out warnings, quiet:true);
                    }
                    structType = StructPropertyType.props;
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Failed to read struct at {f.file_path}({startPos}) with type {typeName}. Skipping...");
                structType = StructPropertyType.error;
                ms.position = endPos;
            }

            //Always jump to the end in case this failed to read correctly.
            if(!isArray)
                ms.position = endPos;
        }

        public UProperty GetPropByName(string name)
        {
            foreach(UProperty p in props)
            {
                if (p.name == name)
                    return p;
            }
            return null;
        }

        public T GetPropByName<T>(string name)
        {
            //Get it 
            UProperty p = GetPropByName(name);

            //If null, return that.
            if (p == null)
                return default(T);

            //Convert
            return (T)Convert.ChangeType(p, typeof(T));
        }
    }

    public enum StructPropertyType
    {
        props, //Use the props property
        predefined, //Use the structData property
        error, //Error
        skipped
    }
}
