using ArkUassetReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.ClassConverter
{
    public class ArkConvertFlag : Attribute
    {
        public string property_name;
        public PropertyTypeIndex type;
        public bool throwOnMissing;
        public object defaultValue;

        public ArkConvertFlag(string property_name, PropertyTypeIndex type, bool throwOnMissing, object defaultValue)
        {
            this.property_name = property_name;
            this.type = type;
            this.throwOnMissing = throwOnMissing;
            this.defaultValue = defaultValue;
        }
    }
}
