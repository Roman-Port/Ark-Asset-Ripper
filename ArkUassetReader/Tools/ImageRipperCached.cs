using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkUassetReader.Tools
{
    public class ImageRipperCached
    {
        /// <summary>
        /// Maps paths to our generated image IDs.
        /// </summary>
        private static Dictionary<string, ArkImageAsset> itemMap = new Dictionary<string, ArkImageAsset>();

        /// <summary>
        /// Used IDs to avoid.
        /// </summary>
        private static List<string> usedIds = new List<string>();

        /// <summary>
        /// Queues
        /// </summary>
        /// <param name="gamePath"></param>
        /// <returns></returns>
        public static ArkImageAsset QueueImage(string gamePath)
        {
            ArkImageAsset asset;

            lock (itemMap)
            {
                //Check if we already have this object.
                if (itemMap.ContainsKey(gamePath))
                    return itemMap[gamePath];

                //We don't have this object. Insert it into the map. Generate an ID
                string id = GenerateUniqueId();
                string thumb_id = GenerateUniqueId();

                //Create an object
                asset = new ArkImageAsset
                {
                    id = id,
                    id_thumb = thumb_id,
                    icon_url = $"https://ark.romanport.com/resources/converted/{id}.png",
                    icon_url_thumb = $"https://ark.romanport.com/resources/converted/{thumb_id}.png"
                };

                //Insert into cache
                itemMap.Add(gamePath, asset);
            }
            

            //Return asset
            return asset;
        }

        public static void ReimportQueue(string path)
        {
            itemMap = JsonConvert.DeserializeObject<Dictionary<string, ArkImageAsset>>(File.ReadAllText(path + "ids.map"));
            usedIds = new List<string>();
            foreach (string k in itemMap.Keys)
            {
                usedIds.Add(k);
                itemMap[k].converted = true;
            }
        }

        /// <summary>
        /// Process the queue and generate files.
        /// </summary>
        public static void ProcessQueue(string output)
        {
            Parallel.For(0, itemMap.Count, (int i) =>
            {
                try
                {
                    DateTime start = DateTime.UtcNow;
                    var item = itemMap.ElementAt(i);
                    if (!File.Exists(output +item.Value.id + ".png"))
                    {
                        string path = item.Key;
                        ArkImageAsset asset = item.Value;
                        asset.converted = true;
                        Program.OpenUAssetImageAndConvert(path, output + asset.id + ".png", output + asset.id_thumb + ".png");
                        GoodWrite($"Converted image in {Math.Round((DateTime.UtcNow - start).TotalMilliseconds, 2)} ms.");
                    }
                    
                    
                } catch (Exception ex)
                {
                    WarningWrite($"Warning: Failed to convert image asset; {ex.Message}");
                }
            });

            //Save map for reimporting
            File.WriteAllText(output + "ids.map", JsonConvert.SerializeObject(itemMap));
        }

        private static string GenerateUniqueId()
        {
            string id;
            lock(usedIds)
            {
                id = Program.GenerateRandomString(24);
                while (usedIds.Contains(id))
                    id = Program.GenerateRandomString(24);
            }
            return id;
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

    public class ArkImageAsset
    {
        public string icon_url;
        public string icon_url_thumb;

        public string id;
        public string id_thumb;

        public bool converted;
    }
}
