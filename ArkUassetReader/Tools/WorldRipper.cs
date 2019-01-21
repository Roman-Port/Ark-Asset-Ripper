using ArkUassetReader.Entities;
using ArkUassetReader.HighLevelEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Tools
{
    public static class WorldRipper
    {
        public static void RipDinoStatus(ref ArkWorldSettings level, string f)
        {
            List<UProperty> primary = UAssetFile.OpenFromFile(f, Program.ARK_GAME_DIR).GetBlueprintProperties();
            List<UProperty> secondary = null;//UAssetFile.OpenFromFile(@"E:\TempFiles\ark\TestGameMode.uasset", Program.ARK_GAME_DIR).GetBlueprintProperties();

            level.additiveTamingBonusModifier = DinoRipper.ReadStatsArray("PerLevelStatsMultiplier_DinoTamed_Add", primary, secondary, DEFAULT_additiveTamingBonusModifier);
            level.multiplicativeTamingBonusModifier = DinoRipper.ReadStatsArray("PerLevelStatsMultiplier_DinoTamed_Affinity", primary, secondary, DEFAULT_multiplicativeTamingBonusModifier);
            level.increasePerTamedLevelModifier = DinoRipper.ReadStatsArray("PerLevelStatsMultiplier_DinoTamed", primary, secondary, DEFAULT_increasePerTamedLevelModifier);
        }

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

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_additiveTamingBonusModifier = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.14f },
            { DinoStatTypeIndex.Stamina, 0f },
            { DinoStatTypeIndex.Torpidity, 0f },
            { DinoStatTypeIndex.Oxygen, 0f },
            { DinoStatTypeIndex.Food, 0f },
            { DinoStatTypeIndex.Water, 0f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0f },
            { DinoStatTypeIndex.MeleeDamage, 0.14f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_multiplicativeTamingBonusModifier = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.44f },
            { DinoStatTypeIndex.Stamina, 0f },
            { DinoStatTypeIndex.Torpidity, 0f },
            { DinoStatTypeIndex.Oxygen, 0f },
            { DinoStatTypeIndex.Food, 0f },
            { DinoStatTypeIndex.Water, 0f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0f },
            { DinoStatTypeIndex.MeleeDamage, 0.44f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };

        public static readonly Dictionary<DinoStatTypeIndex, float> DEFAULT_increasePerTamedLevelModifier = new Dictionary<DinoStatTypeIndex, float> {
            { DinoStatTypeIndex.Health, 0.2f },
            { DinoStatTypeIndex.Stamina, 0f },
            { DinoStatTypeIndex.Torpidity, 0f },
            { DinoStatTypeIndex.Oxygen, 0f },
            { DinoStatTypeIndex.Food, 0f },
            { DinoStatTypeIndex.Water, 0f },
            { DinoStatTypeIndex.Temperature, 0f },
            { DinoStatTypeIndex.Weight, 0f },
            { DinoStatTypeIndex.MeleeDamage, 0.17f },
            { DinoStatTypeIndex.Speed, 0f },
            { DinoStatTypeIndex.TemperatureFortitude, 0f },
            { DinoStatTypeIndex.CraftingSpeed, 0f }
        };
    }
}
