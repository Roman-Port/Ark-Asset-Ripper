using ArkUassetReader.Entities;
using ArkUassetReader.HighLevelEntities;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using ArkUassetReader.Entities.Properties;
using System.Threading.Tasks;
using System.Timers;

namespace ArkUassetReader
{
    /// <summary>
    /// Reads UAssets from the Ark Dev Kit to automate data extraction.
    /// </summary>
    class Program
    {
        //Config

        public const string ARK_GAME_DIR = @"E:/Programs/ARKEditor/Projects/ShooterGame/Content";
        public const string UMODEL_PATH = @"E:\ark_map\GameDataExtraction\libs\umodel\microsoft_sucks.bat";
        public const string TEMP_FOLDER = @"E:\ark_map\GameDataExtraction\temp";
        public const string OUTPUT_PATH = @"E:\ark_map\GameDataExtraction\output\";

        public const string GAME_MODE_PATH = @"E:\TempFiles\ark\TestGameMode.uasset";
        public const string CORE_MEDIA_PRIMAL_GAME_DATA_PATH = @"E:\TempFiles\ark\COREMEDIA_PrimalGameData_BP.uasset";
        public const string BASE_PRIMAL_GAME_DATAA_PATH = @"E:\TempFiles\ark\BASE_PrimalGameData_BP.uasset";

        //Vars
        public static Random rand = new Random();

        //Entry

        static void Main(string[] args)
        {
            //UAssetFile.debug_mode_on = true;
            //UAssetFile f = UAssetFile.OpenFromFile(, ARK_GAME_DIR);

            //Get world
            ArkWorldSettings world = new ArkWorldSettings();
            Tools.WorldRipper.RipDinoStatus(ref world, GAME_MODE_PATH);
            File.WriteAllText(OUTPUT_PATH + "world.json", JsonConvert.SerializeObject(world));

            //Run task
            //List<ArkItemEntry> items = Tasks.CreateItemListTask.DoTask(BASE_PRIMAL_GAME_DATAA_PATH);
            //File.WriteAllText(OUTPUT_PATH + "items.json", JsonConvert.SerializeObject(items));
            List<ArkDinoEntry> dinos = Tasks.CreateDinoListTask.CreateDinoList(CORE_MEDIA_PRIMAL_GAME_DATA_PATH);
            File.WriteAllText(OUTPUT_PATH + "dinos.json", JsonConvert.SerializeObject(dinos));

            //Process queue
            Tools.ImageRipperCached.ProcessQueue(Program.OUTPUT_PATH+"img\\");

            Console.WriteLine("Done.");
            Console.ReadLine();

            //Get dino
            /*
            ArkDinoEntry dino = OpenDino(@"E:\TempFiles\ark\Yutyrannus_Character_BP.uasset");
            //Console.WriteLine(JsonConvert.SerializeObject(dino, Formatting.Indented)+"\n\n");

            TestCalculate(dino, DinoStatTypeIndex.Health, 0x18, 1f, 3, 0, world);
            TestCalculate(dino, DinoStatTypeIndex.Stamina, 20, 1f, 1, 0, world);
            //TestCalculate(dino, DinoStatTypeIndex.Torpidity, 24, 1f, 0, world);
            TestCalculate(dino, DinoStatTypeIndex.Oxygen, 0x1B, 1f, 0, 0, world);
            TestCalculate(dino, DinoStatTypeIndex.Food, 0x17, 1f, 0, 0, world);
            //TestCalculate(dino, DinoStatTypeIndex.Water, 24, 1f, 0, world);
            //TestCalculate(dino, DinoStatTypeIndex.Temperature, 24, 1f, 0, world);
            TestCalculate(dino, DinoStatTypeIndex.Weight, 0x0F, 1f, 0, 0, world);
            TestCalculate(dino, DinoStatTypeIndex.MeleeDamage, 0x11, 1f, 0, 0, world);
            TestCalculate(dino, DinoStatTypeIndex.Speed, 0x17, 1f, 0, 0, world);
            //TestCalculate(dino, DinoStatTypeIndex.TemperatureFortitude, 24, 1f, 0, world);
            //TestCalculate(dino, DinoStatTypeIndex.CraftingSpeed, 24, 1f, 0, world);
            //TestCalculate(dino, DinoStatTypeIndex.Health, 10, 1f, 0);

            Console.WriteLine("\n\n");
            //TestCalculateRex();
            Console.ReadLine();*/
        }

        public delegate bool SeekFilesCheck(string p);
        public static List<string> SeekFiles(string path, SeekFilesCheck check)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            List<string> output = new List<string>();

            foreach(string f in files)
            {
                if (check(f))
                    output.Add(f);
            }

            foreach (string d in dirs)
                output.AddRange(SeekFiles(d, check));

            return output;
        }

        static void TestCalculate(ArkDinoEntry dino_entry, DinoStatTypeIndex stat, float wildLevelups, float tamingEffectiveness, float tamedLevelups, float imprintingBonus, ArkWorldSettings world)
        {
            //As of 1-19-2018 (https://ark.gamepedia.com/Creature_Stats_Calculation)
            //Calculate wild level
            float B = dino_entry.baseLevel[stat];//Base level for Yutyrannus: 1100.0
            float Lw = wildLevelups; //Blake's levelups: 1
            float Iw = dino_entry.increasePerWildLevel[stat];//Increase per wild level: 0.2
            float IwM = world.increasePerWildLevelModifier; //Server modifier: 1
            double Vw = CalculateVw(B, Lw, Iw, IwM);

            //Calculate wild taming effectiveness (Vpt)
            float TBHM = world.serverConfig_tamedBaseHealthMultiplier; //Server config var TamedBaseHealthMultiplier, 1
            float Ta = dino_entry.additiveTamingBonus[stat]; //For health as a test. This is in UE file TamingMaxStatAdditions
            float TaM = world.additiveTamingBonusModifier[stat]; //Seems to be on a per stat basis. UNKNOWN IN FILE TODO: RIP FROM FILE
            float TE = tamingEffectiveness; //Taming effectiveness percentage. Per creature
            float Tm = dino_entry.multiplicativeTamingBonus[stat]; //Multiplier taming bonus. This in the UE file is TamingMaxStatMultipliers 
            float TmM = world.multiplicativeTamingBonusModifier[stat]; //Seems to be on a per stat basis. UNKNOWN IN FILE TODO: RIP FROM FILE
            float IB = imprintingBonus; //Imprint
            float IBM = world.serverConfig_babyImprintingStatScaleMultiplier;
            double Vpt = CalculateVptWild(Vw, TBHM, Ta, TaM, TE, Tm, TmM);

            //Finally, calculate the final result with the user tamed levelups
            float Ld = tamedLevelups; //Number of player applied upgrades
            float Id = dino_entry.increasePerTamedLevel[stat]; //Increase per wild level
            float IdM = world.increasePerTamedLevelModifier[stat]; //Seems to be on a per stat basis. UNKNOWN IN FILE TODO: RIP FROM FILE
            double V = CalculateV(Vpt, Ld, Id, IdM);
            //V = (B * (1 + Lw * Iw * IwM) * TBHM * (1 + IB * 0.2 * IBM) + Ta * TaM) * (1 + TE * Tm * TmM) * (1 + Ld * Id * IdM);

            //Calculating bonus levels with taming effectiveness (Vpt)
            Console.WriteLine($"{stat.ToString()} - {V} - {Math.Round(V, 1)} (Base {B}, Vw {Vw}, Vpt {Vpt})");
        }

        static void TestCalculateRex()
        {
            //As of 1-19-2018 (https://ark.gamepedia.com/Creature_Stats_Calculation)
            //Calculate wild level
            float B = 1000.0f;//Base level for Yutyrannus: 1100.0
            float Lw = 5f; //Blake's levelups: 1
            float Iw = 0.2f;//Increase per wild level: 0.2
            float IwM = 1;//Server modifier: 1
            double Vw = CalculateVw(B, Lw, Iw, IwM);

            //Calculate wild taming effectiveness (Vpt)
            float TBHM = 1; //Server config var TamedBaseHealthMultiplier, 1
            float Ta = 0.5f; //For health as a test. This is in UE file TamingMaxStatAdditions
            float TaM = 0.14f; //Seems to be on a per stat basis. UNKNOWN IN FILE
            float TE = 0.0f; //Taming effectiveness percentage. Per creature
            float Tm = 0.0f; //Multiplier taming bonus. This in the UE file is TamingMaxStatMultipliers
            float TmM = 0.44f; //Seems to be on a per stat basis. UNKNOWN IN FILE
            double Vpt = CalculateVptWild(Vw, TBHM, Ta, TaM, TE, Tm, TmM);

            //Finally, calculate the final result with the user tamed levelups
            float Ld = 12f; //Number of player applied upgrades
            float Id = 0.27f; //Increase per wild level
            float IdM = 0.2f; //Seems to be on a per stat basis. UNKNOWN IN FILE
            double V = CalculateV(Vpt, Ld, Id, IdM);

            //Calculating bonus levels with taming effectiveness (Vpt)
            Console.WriteLine(V);
        }

        static double CalculateVw(float B, float Lw, float Iw, float IwM)
        {
            return B * (1 + Lw * Iw * IwM);
        }

        static double CalculateVptWild(double Vw, float TBHM, float Ta, float TaM, float TE, float Tm, float TmM)
        {
            return (Vw * TBHM + Ta * TaM) * (1 + TE * Tm * TmM);
        }

        static double CalculateV(double Vpt, float Ld, float Id, float IdM)
        {
            //Console.WriteLine($"{Vpt} {Ld} {Id} {IdM}");
            return Vpt * (1 + Ld * Id * (1 - IdM));
        }

        
        static List<string> SeekFolder(string path, string prefixSearch)
        {
            List<string> output = new List<string>();
            
            //Find items
            string[] items = Directory.GetFiles(path);
            foreach (string item in items)
            {
                string name = item.Substring(path.TrimEnd('\\').Length + 1);
                if (name.StartsWith(prefixSearch))
                {
                    string finalItem = (@"\Game" + item.Substring(ARK_GAME_DIR.Length)).Replace('\\', '/').Replace(".uasset", "");
                    output.Add(finalItem);
                }
            }

            //Seek sub directories
            string[] subDirs = Directory.GetDirectories(path);
            foreach (string s in subDirs)
                output.AddRange(SeekFolder(s, prefixSearch));

            return output;
        }

        static void SeekFolderDict(string path, string prefix, ref List<Tuple<string, string>> d, ref List<string> foundPaths)
        {
            //Seek each item and sub directory for names.
            List<string> paths = SeekFolder(path, prefix);

            //Add
            foreach (string s in paths)
            {
                if (!foundPaths.Contains(s))
                    d.Add(new Tuple<string, string>(s, null));
            }
        }

        static List<Tuple<string, string>> ReadPrimalGameData(string path)
        {
            UAssetFile f = UAssetFile.OpenFromFile(path, ARK_GAME_DIR);

            List<Tuple<string, string>> output = new List<Tuple<string, string>>();
            var array = f.GetPropertyByName<ArrayProperty>("MasterItemList", true, true);
            if (array != null)
            {
                foreach (var a in array.items)
                {
                    try
                    {
                        ObjectProperty prop = (ObjectProperty)a;
                        var r = f.GetReferencedObjectById(prop.objectIndex);

                        //If the index is not - still, go down to the next level
                        string className = r.name;
                        if (r.index < 0)
                            r = f.GetReferencedObjectById(r.index);

                        if (!className.ToLower().StartsWith("engram"))
                            output.Add(new Tuple<string, string>(r.name, className));
                        else
                            Console.WriteLine($"Skipping {className}...");
                    }
                    catch
                    {
                        //Ignore...
                    }
                }
            }
            return output;
        }

        //Type specific

        static ArkItem ProcessItem(string f, string knownClassName = null)
        {
            ArkItem i = ArkItem.ReadArkItem(f, knownClassName);
            return i;
        }

        //Helpers

        public static string GenerateRandomString(int length)
        {
            string output = "";
            char[] chars = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
            for (int i = 0; i < length; i++)
            {
                output += chars[rand.Next(0, chars.Length)];
            }
            return output;
        }

        public static string GenerateRandomUniqueFileString(string path, string extension)
        {
            string rand = GenerateRandomString(16);
            while (File.Exists(path + rand + "." + extension))
                rand = GenerateRandomString(16);
            return path + rand + "." + extension;
        }

        public static void OpenUAssetImageAndConvert(string UAssetPath, string outputPath, string outputPathThumb)
        {
            //Get name
            string[] splitUAssetPath = UAssetPath.Split('.');
            string imageName = splitUAssetPath[splitUAssetPath.Length - 2];
            imageName = imageName.Split('/')[imageName.Split('/').Length - 1];

            //Copy here so it doesn't look at all of the other ARK files
            string temporaryUassetName = TEMP_FOLDER + "\\" + imageName + ".uasset";
            if (File.Exists(temporaryUassetName))
                File.Delete(temporaryUassetName);
            File.Copy(UAssetPath, temporaryUassetName);

            //Create command line args for UModel
            string cmd = $"{TEMP_FOLDER} {temporaryUassetName}";

            //Run UModel
            Process p = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                Arguments = cmd,
                FileName = UMODEL_PATH,
                RedirectStandardOutput = false,
                WorkingDirectory = @"E:\ark_map\GameDataExtraction\libs\umodel\"
            });

            //Wait for the process to finish
            p.WaitForExit();

            //Wait a little for Windows to catch up
            System.Threading.Thread.Sleep(800);

            //Convert the TGA image
            string tgaPath = TEMP_FOLDER + "\\" + imageName + ".tga";
            ConvertTgaImage(tgaPath, outputPath, outputPathThumb);

            //Delete temporary files
            File.Delete(tgaPath);
            File.Delete(temporaryUassetName);
        }

        static void ConvertTgaImage(string inPath, string outputPath, string outputPathThumb)
        {
            //Convert
            MemoryStream converted = TgaConverter.ConvertTga(inPath, out MemoryStream thumbOut);

            //Save
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                converted.CopyTo(fs);
            using (FileStream fs = new FileStream(outputPathThumb, FileMode.Create))
                thumbOut.CopyTo(fs);

            //Close MemoryStream
            converted.Close();
            converted.Dispose();
        }

        //Debug
        
        public static void DebugUAsset(UAssetFile uf)
        {
            foreach (UProperty s in uf.GetBlueprintProperties(false))
            {
                Console.WriteLine($"{s.name} ({s.type})");
                Console.WriteLine("    " + JsonConvert.SerializeObject(s));

            }
            Console.WriteLine("Getting ref");
        }
    }
}
