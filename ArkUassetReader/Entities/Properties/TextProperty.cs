
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class TextProperty : UProperty
    {
        public string data;

        public TextProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.data = Convert.ToBase64String(ms.ReadBytes(length));
        }
    }
}
