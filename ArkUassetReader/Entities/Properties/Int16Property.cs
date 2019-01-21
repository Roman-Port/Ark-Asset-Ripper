
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class Int16Property : UProperty
    {
        public Int16 data;

        public Int16Property(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.data = ms.ReadShort();
        }
    }
}
