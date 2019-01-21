using ArkUassetReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ArkUassetReader.Entities.Properties;

namespace ArkUassetReader.ClassConverter
{
    public static class ArkToClassConverter
    {
        public static T ConvertClass<T>(List<List<UProperty>> sources, List<List<UProperty>> fallbacks)
        {
            //Convert a class
            Type t = typeof(T);
            object output = Activator.CreateInstance(t);

            //Loop through each property flagged and try to get it.
            foreach(var prop in t.GetProperties())
            {
                //Check if we have the attrib
                var customAttribs = prop.CustomAttributes.ToArray();
                System.Reflection.CustomAttributeData attrib = null;
                foreach(var a in customAttribs)
                {
                    if (a.AttributeType == typeof(ArkConvertFlag))
                        attrib = a;
                }
                if (attrib == null)
                    continue; //Skip.

                //Get the data from the attribute
                var attribItems = attrib.ConstructorArguments.ToArray();
                string property_name = (string)attribItems[0].Value;
                PropertyTypeIndex type = (PropertyTypeIndex)attribItems[1].Value;
                bool throwOnMissing = (bool)attribItems[2].Value;
                object defaultValue = attribItems[3].Value;

                //Now, get the file it is trying to get and decode it.
                object o = DecodeProp(sources, property_name, type, throwOnMissing, defaultValue);
                prop.SetValue(output, o);
            }

            return (T)output;
        }

        static object DecodeProp(List<List<UProperty>> data, string property_name, PropertyTypeIndex type, bool throwOnMissing, object defaultValue)
        {
            //First, try to find a property in primary or secondary that has this.
            UProperty p = null;
            foreach(var primary in data)
            {
                foreach (var prop in primary)
                {
                    if (prop.name == property_name)
                        p = prop;
                }
                if (p != null)
                    break;
            }
            if(p == null)
            {
                if (throwOnMissing)
                    throw new Exception($"Required property '{property_name}' was not found.");
                else
                    return defaultValue;
            }

            //Now that we have a valid prop, validate the type
            if (type.ToString() != p.type)
                throw new Exception($"Property type mismatch: Type of '{property_name}', '{p.type}', did not match type requested by class, '{type.ToString()}.");

            //Decode
            switch(type)
            {
                case PropertyTypeIndex.BoolProperty:
                    return ((BoolProperty)p).flag;
                case PropertyTypeIndex.ByteProperty:
                    return ((ByteProperty)p).byteValue;
                case PropertyTypeIndex.DoubleProperty:
                    return ((DoubleProperty)p).data;
                case PropertyTypeIndex.FloatProperty:
                    return ((FloatProperty)p).data;
                case PropertyTypeIndex.Int16Property:
                    return ((Int16Property)p).data;
                case PropertyTypeIndex.Int8Property:
                    return ((Int8Property)p).data;
                case PropertyTypeIndex.IntProperty:
                    return ((IntProperty)p).data;
                case PropertyTypeIndex.NameProperty:
                    return ((NameProperty)p).name_string;
                case PropertyTypeIndex.StrProperty:
                    return ((StrProperty)p).data;
                case PropertyTypeIndex.TextProperty:
                    return ((TextProperty)p).data;
                case PropertyTypeIndex.UInt16Property:
                    return ((UInt16Property)p).data;
                case PropertyTypeIndex.UInt32Property:
                    return ((UInt32Property)p).data;
                case PropertyTypeIndex.UInt64Property:
                    return ((UInt64Property)p).data;
                default:
                    throw new Exception($"Property of type '{type.ToString()}' is not supported at this time.");
            }
        }
    }
}
