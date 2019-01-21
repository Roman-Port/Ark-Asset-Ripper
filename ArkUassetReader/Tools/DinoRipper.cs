using ArkUassetReader.Entities;
using ArkUassetReader.Entities.Properties;
using ArkUassetReader.HighLevelEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Tools
{
    public static class DinoRipper
    {
        public static void RipDino(string pathname, List<UProperty> default_props)
        {

        }

        public static void RipDinoStatus(ref ArkDinoEntry dino, List<UProperty> primary)
        {
            List<UProperty> secondary = UAssetFile.OpenFromFile(@"E:\TempFiles\ark\DinoCharacterStatusComponent_BP.uasset", Program.ARK_GAME_DIR).GetBlueprintProperties();

            dino.baseLevel = ReadStatsArray("MaxStatusValues", primary, secondary, DEFAULT_BASE_LEVEL);
            dino.increasePerWildLevel = ReadStatsArray("AmountMaxGainedPerLevelUpValue", primary, secondary, DEFAULT_INCREASE_PER_WILD_LEVEL);
            dino.increasePerTamedLevel = ReadStatsArray("AmountMaxGainedPerLevelUpValueTamed", primary, secondary, DEFAULT_INCREASE_PER_TAMED_LEVEL);

            dino.additiveTamingBonus = ReadStatsArray("TamingMaxStatAdditions", primary, secondary, DEFAULT_TAMING_MAX_STAT_ADDITIONS);
            dino.multiplicativeTamingBonus = ReadStatsArray("TamingMaxStatMultipliers", primary, secondary, DEFAULT_TAMING_MAX_STAT_MULTIPLY);
        }

        static UProperty GetPropertyBaseByName(string name, List<UProperty> primary, List<UProperty> secondary)
        {
            //Try to get the UProperty from primary, but if not, use secondary.
            UProperty output = null;
            foreach(UProperty u in primary)
            {
                if (u.name == name)
                    output = u;
            }

            //If that failed, get from secondary
            if(output == null)
            {
                foreach(UProperty u in secondary)
                {
                    if (u.name == name)
                        output = u;
                }
            }

            //If null, throw
            if (output == null)
                throw new Exception("Required field missing.");
            return output;
        }

        public static Dictionary<DinoStatTypeIndex, float> ReadStatsArray(string propertyName, List<UProperty> primary, List<UProperty> secondary, Dictionary<DinoStatTypeIndex, float> defaults)
        {
            return new Dictionary<DinoStatTypeIndex, float>
            {
                { DinoStatTypeIndex.Health, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Health, primary, secondary, defaults) },
                { DinoStatTypeIndex.Stamina, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Stamina, primary, secondary, defaults) },
                { DinoStatTypeIndex.Torpidity, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Torpidity, primary, secondary, defaults) },
                { DinoStatTypeIndex.Oxygen, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Oxygen, primary, secondary, defaults) },
                { DinoStatTypeIndex.Food, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Food, primary, secondary, defaults) },
                { DinoStatTypeIndex.Water, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Water, primary, secondary, defaults) },
                { DinoStatTypeIndex.Temperature, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Temperature, primary, secondary, defaults) },
                { DinoStatTypeIndex.Weight, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Weight, primary, secondary, defaults) },
                { DinoStatTypeIndex.MeleeDamage, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.MeleeDamage, primary, secondary, defaults) },
                { DinoStatTypeIndex.Speed, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.Speed, primary, secondary, defaults) },
                { DinoStatTypeIndex.TemperatureFortitude, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.TemperatureFortitude, primary, secondary, defaults) },
                { DinoStatTypeIndex.CraftingSpeed, ReadFloatPropertyIndexedArrayValue(propertyName, DinoStatTypeIndex.CraftingSpeed, primary, secondary, defaults) }
            };
        }

        static float ReadFloatPropertyIndexedArrayValue(string propertyName, DinoStatTypeIndex index, List<UProperty> primary, List<UProperty> secondary, Dictionary<DinoStatTypeIndex, float> defaults)
        {
            //Find by index in first 
            UProperty finalProp = null;
            foreach(var prop in primary)
            {
                if (prop.name == propertyName && prop.index == (int)index)
                    finalProp = prop;
            }

            //If that failed, get from secondary
            if(finalProp == null && secondary != null)
            {
                foreach (var prop in secondary)
                {
                    if (prop.name == propertyName && prop.index == (int)index)
                        finalProp = prop;
                }
            }

            //If this is null, fall back to defaults
            if(finalProp == null)
            {
                if (defaults.ContainsKey(index))
                    return defaults[index];
                else
                    throw new IndexOutOfRangeException();
            }

            //Convert to float
            return ((FloatProperty)finalProp).data;
        }

        /*
         * DEFAULTS
         */

        public static readonly Dictionary<DinoStatTypeIndex, float> BASE_DEFAULT = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0f },
            { DinoStatTypeIndex.Stamina, 0f },
            { DinoStatTypeIndex.Torpidity, 0f },
            { DinoStatTypeIndex.Oxygen, 0f },
            { DinoStatTypeIndex.Food, 0f },
            { DinoStatTypeIndex.Water, 0f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0f },
            { DinoStatTypeIndex.MeleeDamage, 0f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_BASE_LEVEL = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 100f },
            { DinoStatTypeIndex.Stamina, 100f },
            { DinoStatTypeIndex.Torpidity, 100f },
            { DinoStatTypeIndex.Oxygen, 150f },
            { DinoStatTypeIndex.Food, 100f },
            { DinoStatTypeIndex.Water, 100f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 100f },
            { DinoStatTypeIndex.MeleeDamage, 0f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_INCREASE_PER_WILD_LEVEL = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.2f },
            { DinoStatTypeIndex.Stamina, 0.1f },
            { DinoStatTypeIndex.Torpidity, 0f },
            { DinoStatTypeIndex.Oxygen, 0.1f },
            { DinoStatTypeIndex.Food, 0.1f },
            { DinoStatTypeIndex.Water, 0.1f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0.02f },
            { DinoStatTypeIndex.MeleeDamage, 0.05f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_INCREASE_PER_TAMED_LEVEL = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.2f },
            { DinoStatTypeIndex.Stamina, 0.1f },
            { DinoStatTypeIndex.Torpidity, 0f },
            { DinoStatTypeIndex.Oxygen, 0.1f },
            { DinoStatTypeIndex.Food, 0.1f },
            { DinoStatTypeIndex.Water, 0.1f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0.04f },
            { DinoStatTypeIndex.MeleeDamage, 0.1f },
            { DinoStatTypeIndex.Speed, 0.01f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_TAMING_MAX_STAT_ADDITIONS = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.5f },
            { DinoStatTypeIndex.Stamina, 0f },
            { DinoStatTypeIndex.Torpidity, 0.5f },
            { DinoStatTypeIndex.Oxygen, 0f },
            { DinoStatTypeIndex.Food, 0f },
            { DinoStatTypeIndex.Water, 0f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0f },
            { DinoStatTypeIndex.MeleeDamage, 0.5f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_TAMING_MAX_STAT_MULTIPLY = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.0f },
            { DinoStatTypeIndex.Stamina, 0f },
            { DinoStatTypeIndex.Torpidity, 0.0f },
            { DinoStatTypeIndex.Oxygen, 0f },
            { DinoStatTypeIndex.Food, 0f },
            { DinoStatTypeIndex.Water, 0f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0f },
            { DinoStatTypeIndex.MeleeDamage, 0.4f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };
    }
}
