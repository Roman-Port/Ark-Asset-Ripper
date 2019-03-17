using ArkMapper.Entities;
using ArkUassetReader.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArkMapper
{
    class Program
    {
        const string SEARCH_DIR = @"E:/Programs/ARKEditor/Projects/ShooterGame/Content";
        const string GAME_ROOT = @"E:/Programs/ARKEditor/Projects/ShooterGame/Content";

        static void Main(string[] args)
        {
            Console.WriteLine("Ark Mapper - Creates a map of all of the Ark assets so we know what is what");

            //Now that we have all of the content loaded, find roots (items that have no parents in the file)

            List<StageOneItem> data = JsonConvert.DeserializeObject < List < StageOneItem > > (File.ReadAllText("stageone_output.json"));
            string output = "";
            StageOneItem stageTwoData = new StageOneItem();
            stageTwoData.children = new List<StageOneItem>();
            FindRootsOfName("/Script/ShooterGame", 0, data, ref output, stageTwoData);

            File.WriteAllText("stagetwo_output.txt", output);
            File.WriteAllText("stagetwo_output.json", JsonConvert.SerializeObject(stageTwoData, Formatting.Indented));

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void FindRootsOfName(string root, int depth, List<StageOneItem> data, ref string output, StageOneItem outputJson)
        {
            //Find all
            var items = data.Where(x => x.parentPackagePath == root).ToArray();

            //Create padding
            string padding = "";
            for (int i = 0; i < depth; i++)
                padding += "├ ";

            //Write all
            for (int i = 0; i<items.Length; i++)
            {
                output += (padding+"---"+items[i].classname+"\n");
                StageOneItem activeItem = items[i];
                if (activeItem.children == null)
                    activeItem.children = new List<StageOneItem>();
                FindRootsOfName(items[i].packagePath, depth + 1, data, ref output, activeItem);
                outputJson.children.Add(activeItem);
            }
        }

        static List<StageOneItem> StepOne()
        {
            Console.WriteLine("\nStep 1: Searching for files");
            List<string> files = ArkUassetReader.Program.SeekFiles(SEARCH_DIR, (string p) =>
            {
                return p.EndsWith(".uasset");
            });
            Console.WriteLine("Found " + files.Count + " files.");

            //Open all and pull the initial data. This will take the longest.
            Console.WriteLine("\nStep 2: Reading files\n");
            int read = 1; //I'm lazy.
            int fatalErrors = 0;
            int noPackageErrors = 0;
            List<StageOneItem> stageOneItems = new List<StageOneItem>();
            DateTime startTime = DateTime.UtcNow;
            Parallel.For(0, files.Count, (int i) =>
            {
                try
                {
                    UAssetFile f = UAssetFile.OpenFromFile(files[i], GAME_ROOT);
                    //Console.WriteLine(f.classname + " - "+ f.package+" - "+f.GetParentPackage());
                    //Console.ReadLine();

                    //We're only looking for Blueprint generated classes. Skip any if they do not have a package path
                    if (f.package != null)
                    {
                        //Create object
                        StageOneItem oi = new StageOneItem
                        {
                            classname = f.classname,
                            filename = files[i],
                            packagePath = f.package,
                            parentPackagePath = f.GetParentPackage()
                        };
                        lock (stageOneItems)
                            stageOneItems.Add(oi);
                    }
                    else
                    {
                        noPackageErrors++;
                    }
                    read++;
                    float doneness = (float)read / (float)files.Count;
                    TimeSpan timePerItem = (DateTime.UtcNow - startTime) / read;
                    TimeSpan timeToDone = timePerItem * (files.Count - read);
                    Console.Title = $"{read} / {files.Count} read ({MathF.Round(doneness * 100)}%); {stageOneItems.Count} ok; {fatalErrors} read errors; {noPackageErrors} cancelled; {timeToDone.Hours}:{timeToDone.Minutes}:{timeToDone.Seconds} remaining";
                    f.Dispose();
                }
                catch (Exception ex)
                {
                    fatalErrors++;
                }
            });

            //Save
            File.WriteAllText("stageone_output.json", JsonConvert.SerializeObject(stageOneItems));

            return stageOneItems;
        }
    }
}
