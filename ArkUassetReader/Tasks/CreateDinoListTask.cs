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
                UAssetFile statusComponent = FindStatusComponent(dino);
                if (statusComponent == null)
                {
                    //Could not be found
                    WarningWrite($"Warning: Could not find status component ref for dino tag '{dino.classname} ({dino.file_path})'. Skipping...");
                    continue;
                }

                //Get BP components
                List<UProperty> dinoProps = dino.GetBlueprintProperties();
                List<UProperty> statusProps = statusComponent.GetBlueprintProperties();

                //Get the settings
                List<ArkDinoFood> childFoods = null;
                List<ArkDinoFood> adultFoods = null;

                //Grab child settings
                var ah = dino.GetPropertyByName("BabyDinoSettings");
                if (ah == null)
                    WarningWrite($"Could not get dino settings (child) for {dino.classname}. Continuing...");
                else
                {
                    try
                    {
                        UAssetFile dinoSettingsAdult = dino.GetReferencedFile(ah);
                        var t = ConvertFoods(dinoSettingsAdult);
                        childFoods = t;
                    }
                    catch (Exception ex)
                    {
                        WarningWrite($"Could not convert dino settings (child) for {dino.classname}. Continuing...");
                    }
                }

                //Grab adult settings
                ah = dino.GetPropertyByName("DinoSettingsClass");
                if(ah == null)
                    WarningWrite($"Could not get dino settings (adult) for {dino.classname}. Continuing...");
                else
                {
                    try
                    {
                        UAssetFile dinoSettingsAdult = dino.GetReferencedFile(ah);
                        var t = ConvertFoods(dinoSettingsAdult);
                        adultFoods = t;
                    } catch (Exception ex)
                    {
                        WarningWrite($"Could not convert dino settings (adult) for {dino.classname}. Continuing...");
                    }
                }
                

                //Now that we have all of the components, convert them

                //Start dino
                try
                {
                    ArkDinoEntry dinoE = ClassConverter.ArkToClassConverter.ConvertClass<ArkDinoEntry>(new List<List<UProperty>> { dinoProps, statusProps }, null);
                    dinoE.statusComponent = ClassConverter.ArkToClassConverter.ConvertClass<ArkDinoEntryStatusComponent>(new List<List<UProperty>> { statusProps }, null);

                    //Fill in missing info
                    dinoE.classname = dino.classname;
                    dinoE.blueprintPath = dino.path;
                    dinoE.icon_url = $"https://ark.romanport.com/resources/dinos/icons/hq/{dinoE.classname}_C.png";
                    dinoE.thumb_icon_url = $"https://ark.romanport.com/resources/dinos/icons/lq/{dinoE.classname}_C.png";

                    //Add foods
                    dinoE.adultFoods = adultFoods;
                    dinoE.childFoods = childFoods;

                    //Read in stats
                    Tools.DinoRipper.RipDinoStatus(ref dinoE, statusProps);

                    //Add to list
                    output.Add(dinoE);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Converted dino {dino.classname} ({dino.file_path})!");
                    Console.ForegroundColor = ConsoleColor.White;
                } catch
                {
                    WarningWrite($"Warning: Failed to final-convert dino '{dino.classname}' ({dino.file_path}).");
                }
            }

            Console.WriteLine($"Converted {output.Count} dinos!");

            return output;
        }

        static UAssetFile FindStatusComponent(UAssetFile workingFile)
        {
            try
            {
                GameObjectTableHead output = null;
                while (workingFile != null)
                {
                    var refObj = workingFile.ref_objects.Where(x => x.name.StartsWith($"DinoCharacterStatusComponent_BP_")).ToArray();
                    if (refObj.Length != 1)
                    {
                        //Did not have the status component. Load the next parent
                        workingFile = workingFile.LoadParentPackage();
                    }
                    else
                    {
                        //We're good.
                        output = refObj[0];
                        break;
                    }
                }
                if (output == null)
                    return null;
                else
                    return workingFile.GetReferencedFileById(output.index);
            } catch
            {
                return null;
            }
        }

        static List<ArkDinoFood> ConvertFoods(UAssetFile f)
        {
            List<ArkDinoFood> foods = new List<ArkDinoFood>();
            foods.AddRange(ConvertFoodsFromDSettings(f.GetPropertyByName<ArrayProperty>("FoodEffectivenessMultipliers", openStructs: true), f));
            if(f.HasProperty("ExtraFoodEffectivenessMultipliers", openStructs:true))
                foods.AddRange(ConvertFoodsFromDSettings(f.GetPropertyByName<ArrayProperty>("ExtraFoodEffectivenessMultipliers", openStructs: true), f));
            return foods;
        }

        static List<ArkDinoFood> ConvertFoodsFromDSettings(ArrayProperty prop, UAssetFile f)
        {
            List<ArkDinoFood> output = new List<ArkDinoFood>();
            //Loop through all structs
            foreach(var i in prop.items)
            {
                var ii = (StructProperty)i;
                if (ii.props.Count != 9)
                    continue;
                ArkDinoFood ff = new ArkDinoFood();
                ff.affinityEffectivenessMultiplier = ii.GetPropByName<FloatProperty>("AffinityEffectivenessMultiplier").data;
                ff.affinityOverride = ii.GetPropByName<FloatProperty>("AffinityOverride").data;
                ff.foodCategory = ii.GetPropByName<IntProperty>("FoodItemCategory").data;
                ff.foodEffectivenessMultiplier = ii.GetPropByName<FloatProperty>("FoodEffectivenessMultiplier").data;
                ff.priority = ii.GetPropByName<FloatProperty>("UntamedFoodConsumptionPriority").data;
                ff.classname = f.GetReferencedObject(ii.GetPropByName("FoodItemParent"), false).name;
                output.Add(ff);

            }
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
                return f.Contains("_Character_BP");
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
