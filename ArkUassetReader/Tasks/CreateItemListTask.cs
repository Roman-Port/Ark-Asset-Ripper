using ArkUassetReader.Entities;
using ArkUassetReader.Entities.Properties;
using ArkUassetReader.HighLevelEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkUassetReader.Tasks
{
    public static class CreateItemListTask
    {
        public static List<ArkItemEntry> DoTask(string primalGameDataPath)
        {
            //Open PrimalGameData
            Console.WriteLine("Opening PrimalGameData...");
            UAssetFile primalGameData = UAssetFile.OpenFromFile(primalGameDataPath, Program.ARK_GAME_DIR);
            List<UProperty> primalGameDataProps = primalGameData.GetBlueprintProperties();

            //Get the master item list
            ArrayProperty masterItemList = (ArrayProperty)UAssetFile.GetPropertyByNameStatic(primalGameDataProps, "MasterItemList");

            //Open every item in the master item list
            List<ArkItemEntry> items = new List<ArkItemEntry>();
            Console.WriteLine($"Found {masterItemList.items.Count} items. Opening assets...");
            
            foreach(var iprop in masterItemList.items)
            {
                UAssetFile itemAsset;
                List<UProperty> itemProps;
                var itemProp = (ObjectProperty)iprop;
                try
                {
                    itemAsset = primalGameData.GetReferencedFile(itemProp, true);
                    itemProps = itemAsset.GetBlueprintProperties();
                }
                catch
                {
                    WarningWrite($"Failed to open item ref with index {itemProp.objectIndex}. Skipping...");
                    continue;
                }
                if (itemAsset.classname.StartsWith("EngramEntry"))
                {
                    WarningWrite($"Skipping engram '{itemAsset.classname}'.");
                }
                else
                {
                    //Now that we have the file, convert it.
                    ArkItemEntry item = ClassConverter.ArkToClassConverter.ConvertClass<ArkItemEntry>(new List<List<UProperty>> { itemProps }, null);

                    //Now convert images
                    try
                    {
                        string iconAbsolutePath = itemAsset.GetFullFilePath(itemAsset.GetReferencedObject(itemAsset.GetPropertyByName("ItemIcon"), true));
                        item.icon = Tools.ImageRipperCached.QueueImage(iconAbsolutePath);
                    }
                    catch
                    {
                        WarningWrite($"Failed to find icon image for item '{itemAsset.classname}'. Item will not have an icon image.");
                    }
                    try
                    {
                        string iconBrokenAbsolutePath = itemAsset.GetFullFilePath(itemAsset.GetReferencedObject(itemAsset.GetPropertyByName("BrokenImage"), true));
                        item.broken_icon = Tools.ImageRipperCached.QueueImage(iconBrokenAbsolutePath);
                    }
                    catch
                    {
                        //This means it is the default. Set it to our own custom path.
                        item.broken_icon = new Tools.ArkImageAsset
                        {
                            icon_url = "https://ark.romanport.com/resources/broken_item.png",
                            icon_url_thumb = "https://ark.romanport.com/resources/broken_item_thumb.png"
                        };
                    }

                    //Set some more values
                    item.classname = itemAsset.classname;
                    item.blueprintPath = itemAsset.path;

                    items.Add(item);
                    GoodWrite($"Converted engram '{itemAsset.classname}'.");
                }
            }

            return items;
        }

        static void WarningWrite(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void GoodWrite(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
