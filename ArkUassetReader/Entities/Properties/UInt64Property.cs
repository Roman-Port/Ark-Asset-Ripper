
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class UInt64Property : UProperty
    {
        public UInt64 data;

        public UInt64Property(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            this.data = ms.ReadULong();
        }
    }
}
