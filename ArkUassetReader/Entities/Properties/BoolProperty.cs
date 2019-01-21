
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class BoolProperty : UProperty
    {
        public bool flag;

        public BoolProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            flag = ms.ReadByte() != 0;
        }
    }
}
