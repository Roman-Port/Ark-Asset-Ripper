﻿
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities.Properties
{
    public class ArrayProperty : UProperty
    {
        public string arrayType;
        public List<UProperty> items;

        public ArrayProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            //Read classname
            arrayType = ms.ReadNameTableEntry(f);

            //If this is not a known good array, ignore
            if(arrayType != "ObjectProperty")
            {
                ms.position += 4 + length;
                return;
            }

            //Skip unknown int
            ms.ReadInt();

            //Get ending byte
            long end = ms.position + length;

            //Read until end
            items = new List<UProperty>();
            while(ms.position<end)
            {
                var p = UProperty.ReadAnyProp(ms, f, out List<string> warnings, true, arrayType);
                items.Add(p);
            }

            //Ensure we're actually at the end
            ms.position = end;
        }
    }
}
