using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class StructProperty : UProperty
    {
        public string structType;
        //public DotArkStruct structData;

        public StructProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            //Read the struct type
            structType = ms.ReadNameTableEntry(f);

            ms.position += length + 4;
        }
    }
}
