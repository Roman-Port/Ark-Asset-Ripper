
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class IntProperty : UProperty
    {
        public Int32 data;

        public IntProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.data = ms.ReadInt();
        }
    }
}
