using ArkUassetReader.Entities;
using ArkUassetReader.Entities.Properties;
using ArkUassetReader.HighLevelEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkUassetReader.Tasks
{
    public static class CreateDinoListTask
    {
        public static List<ArkDinoEntry> CreateDinoList(string primalGameDataPath)
        {
            //Open PrimalGameData
            Console.WriteLine("Opening PrimalGameData...");
            UAssetFile primalGameData = UAssetFile.OpenFromFile(primalGameDataPath, Program.ARK_GAME_DIR);
            List<UProperty> primalGameDataProps = primalGameData.GetBlueprintProperties();

            List<ArkDinoEntry> output = new List<ArkDinoEntry>();

            //Read the DinoEnteries list
            ReadMasterDinoList(primalGameDataProps, primalGameData, out Dictionary<string, UAssetFile> dinoBps);

            //Loop through dino bps and open them.
            foreach(var dino in dinoBps.Values)
            {
                //Open status component 
                //To do this, find the name tag
                string nameTag = dino.GetPropertyByName<NameProperty>("DinoNameTag").name_string;

                //Now, find the ref gameobject table entry with this.
                var refObj = dino.ref_objects.Where(x => x.name.StartsWith($"DinoCharacterStatusComponent_BP_")).ToArray();
                if(refObj.Length != 1)
                {
                    //Could not be found
                    WarningWrite($"Warning: Could not find status component ref for dino tag '{nameTag}'. Skipping...");
                    continue;
                }

                //Find the referenced file
                var statusComponent = dino.GetReferencedFileById(refObj[0].index);

                //Get BP components
                List<UProperty> dinoProps = dino.GetBlueprintProperties();
                List<UProperty> statusProps = statusComponent.GetBlueprintProperties();

                //Now that we have all of the components, convert them

                //Start dino
                try
                {
                    ArkDinoEntry dinoE = ClassConverter.ArkToClassConverter.ConvertClass<ArkDinoEntry>(new List<List<UProperty>> { dinoProps, statusProps }, null);

                    //Fill in missing info
                    dinoE.classname = dino.classname;
                    dinoE.blueprintPath = dino.path;
                    dinoE.icon_url = $"https://ark.romanport.com/resources/dinos/icons/hq/{dinoE.classname}.png";
                    dinoE.thumb_icon_url = $"https://ark.romanport.com/resources/dinos/icons/lq/{dinoE.classname}.png";

                    //Read in stats
                    Tools.DinoRipper.RipDinoStatus(ref dinoE, statusProps);

                    //Add to list
                    output.Add(dinoE);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Converted dino {dinoE.screen_name}!");
                    Console.ForegroundColor = ConsoleColor.White;
                } catch
                {
                    WarningWrite($"Warning: Failed to final-convert dino '{nameTag}'.");
                }
            }

            Console.WriteLine($"Converted {output.Count} dinos!");

            return output;
        }

        static void WarningWrite(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void ReadMasterDinoList(List<UProperty> props, UAssetFile primalGameData, out Dictionary<string, UAssetFile> dinoBps)
        {
            dinoBps = new Dictionary<string, UAssetFile>(); //Key: Classname

            //Now, find all files by the tags used by the dinos. Not ideal :(
            Console.WriteLine("Seeking for dino files...");
            List<string> dinoPaths = Program.SeekFiles(Program.ARK_GAME_DIR, (string f) =>
            {
                //First, check if it ends in "_Character_BP".
                return f.EndsWith("_Character_BP.uasset");
            });

            //Now, open each dino BP.
            foreach(string s in dinoPaths)
            {
                try
                {
                    UAssetFile f = UAssetFile.OpenFromFile(s, Program.ARK_GAME_DIR);
                    if (f.HasProperty("DinoNameTag"))
                    {
                        //Add BP
                        dinoBps.Add(f.classname, f);
                        Console.WriteLine($"Found dino '{f.classname}'.");
                    }
                } catch
                {
                    WarningWrite($"Warning: Failed to open '{s}'. Skipping...");
                }
            }
            Console.WriteLine($"Found {dinoPaths.Count} dinos.");
        }
    }
}
