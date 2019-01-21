
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class UInt32Property : UProperty
    {
        public UInt32 data;

        public UInt32Property(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.data = ms.ReadUInt();
        }
    }
}
