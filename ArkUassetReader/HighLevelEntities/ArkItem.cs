using ArkUassetReader.Entities;
using ArkUassetReader.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.HighLevelEntities
{
    public class ArkItem
    {
        public string displayName;
        public string description;
        public float minimumUseInterval;
        public int maxStackSize;
        public string className;
        public string iconUrl;
        public string iconPath;

        public float spoilingTime;

        public static ArkItem ReadArkItem(string filename, string knownClassName = null)
        {
            //Open UAsset file
            UAssetFile uf = UAssetFile.OpenFromFile(filename, Program.ARK_GAME_DIR);
            ArkItem a = new ArkItem();

            //Check if this has required properties
            if (!uf.HasProperty("DescriptiveNameBase") || !uf.HasProperty("ItemDescription"))
                return null;

            //Get properties
            a.displayName = uf.GetPropertyByName<StrProperty>("DescriptiveNameBase").data;
            a.description = uf.GetPropertyByName<StrProperty>("ItemDescription").data;
            if(uf.HasProperty("MinimumUseInterval"))
                a.minimumUseInterval = uf.GetPropertyByName<FloatProperty>("MinimumUseInterval").data;
            if (uf.HasProperty("SpoilingTime"))
                a.spoilingTime = uf.GetPropertyByName<FloatProperty>("SpoilingTime").data;
            if (uf.HasProperty("MaxItemQuantity"))
                a.maxStackSize = uf.GetPropertyByName<IntProperty>("MaxItemQuantity").data;
            a.className = uf.classname;
            if (knownClassName != null)
                a.className = knownClassName;


            //Get icon
            string iconAbsolutePath = uf.GetFullFilePath(uf.GetReferencedObject(uf.GetPropertyByName("ItemIcon"), true));

            //Generate url and set
            string outputPathname = Program.OUTPUT_PATH + "items\\icons\\" + a.className + ".png";
            string relOutputPathname = outputPathname.Substring(Program.OUTPUT_PATH.TrimEnd('\\').Length).Replace('\\', '/');
            a.iconPath = relOutputPathname;
            a.iconUrl = "https://ark.romanport.com/resources" + relOutputPathname;

            //Convert image
            Program.OpenUAssetImageAndConvert(iconAbsolutePath, outputPathname, outputPathname+".thumb");
            return a;
        }
    }
}
