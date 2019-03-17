using ArkUassetReader.Entities;
using ArkUassetReader.Entities.Properties;
using ArkUassetReader.HighLevelEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkUassetReader.Tasks
{
    public static class CreateItemListTask
    {
        public static List<ArkItemEntry> DoTask()
        {
            //Track down all files
            List<string> filePaths = Program.SeekFiles(Program.ARK_GAME_DIR, (string target) =>
            {
                if (target.Contains("Content\\Mods") || target.Contains("Content/Mods"))
                    return false;
                return target.Contains("PrimalItem");
            });

            List<ArkItemEntry> items = new List<ArkItemEntry>();
            Console.WriteLine($"Found {filePaths.Count} items. Opening assets...");

            //Loop through files
            int completed = 0;
            int failed = 0;
            foreach(var filePath in filePaths)
            {
                UAssetFile itemAsset;
                List<UProperty> itemProps;
                try
                {
                    itemAsset = UAssetFile.OpenFromFile(filePath, Program.ARK_GAME_DIR);
                    itemProps = itemAsset.GetBlueprintProperties(readStructs:true);
                    if (itemAsset.classname.StartsWith("EngramEntry"))
                    {
                        WarningWrite($"Skipping engram '{itemAsset.classname}'.");
                    }
                    else
                    {
                        //Now that we have the file, convert it.
                        ArkItemEntry item = ClassConverter.ArkToClassConverter.ConvertClass<ArkItemEntry>(new List<List<UProperty>> { itemProps }, null);

                        //Add some optional values
                        if (itemProps.Where(x => x.name == "UseItemAddCharacterStatusValues").Count() == 1)
                        {
                            var pi = (ArrayProperty)itemProps.Where(x => x.name == "UseItemAddCharacterStatusValues").ToArray()[0];
                            item.addStatusValues = new Dictionary<string, ArkItemEntry_ConsumableAddStatusValue>();
                            //Loop through each of these.
                            foreach (var pii in pi.items)
                            {
                                var spii = (StructProperty)pii;
                                ArkItemEntry_ConsumableAddStatusValue statusValue = ClassConverter.ArkToClassConverter.ConvertClass<ArkItemEntry_ConsumableAddStatusValue>(new List<List<UProperty>> { spii.props }, null);
                                statusValue.statusValueType = ((ByteProperty)spii.props.Where(x => x.name == "StatusValueType").ToArray()[0]).enumValue;
                                if (!item.addStatusValues.ContainsKey(statusValue.statusValueType))
                                    item.addStatusValues.Add(statusValue.statusValueType, statusValue);
                            }
                        }

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
                        GoodWrite($"Converted item '{itemAsset.classname}'.");
                        
                    }
                }
                catch
                {
                    WarningWrite($"Failed to open asset {filePath}. Skipping...");
                    failed++;
                    continue;
                }
                Console.Title = $"Item conversion - {completed++} / {filePaths.Count} ({failed} failed)";
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
