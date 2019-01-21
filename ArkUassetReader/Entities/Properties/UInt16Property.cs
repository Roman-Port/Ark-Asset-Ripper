
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class UInt16Property : UProperty
    {
        public UInt16 data;

        public UInt16Property(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.data = ms.ReadUShort();
        }
    }
}
