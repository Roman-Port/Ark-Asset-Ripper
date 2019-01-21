
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class StrProperty : UProperty
    {
        public string data;

        public StrProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.data = ms.ReadUEString();
        }
    }
}
