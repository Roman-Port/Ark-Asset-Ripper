
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class NameProperty : UProperty
    {
        public string name_string;

        public NameProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            

            this.name_string = ms.ReadNameTableEntry(f);

            ms.position += 4;
        }
    }
}
