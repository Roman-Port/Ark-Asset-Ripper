using ArkUassetReader.ClassConverter;
using ArkUassetReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.HighLevelEntities
{
    public class ArkDinoEntry
    {
        [ArkConvertFlag("DescriptiveName", PropertyTypeIndex.StrProperty, true, null)]
        public string screen_name { get; set; }
        [ArkConvertFlag("ColorizationIntensity", PropertyTypeIndex.FloatProperty, false, 1)]
        public float colorizationIntensity { get; set; }
        [ArkConvertFlag("BabyGestationSpeed", PropertyTypeIndex.FloatProperty, false, 0.000035f)]
        public float babyGestationSpeed { get; set; }
        [ArkConvertFlag("ExtraBabyGestationSpeedMultiplier", PropertyTypeIndex.FloatProperty, false, 1f)]
        public float extraBabyGestationSpeedMultiplier { get; set; }
        [ArkConvertFlag("BabyAgeSpeed", PropertyTypeIndex.FloatProperty, false, 0.000003f)]
        public float babyAgeSpeed { get; set; }
        [ArkConvertFlag("ExtraBabyAgeSpeedMultiplier", PropertyTypeIndex.FloatProperty, false, 1f)]
        public float extraBabyAgeSpeedMultiplier { get; set; }
        [ArkConvertFlag("bUseBabyGestation", PropertyTypeIndex.BoolProperty, false, false)]
        public bool useBabyGestation { get; set; }

        //New in v2
        public ArkDinoEntryStatusComponent statusComponent;

        //New in v3
        public List<ArkDinoFood> adultFoods;
        public List<ArkDinoFood> childFoods;

        public string classname;
        public string blueprintPath;

        public string icon_url;
        public string thumb_icon_url;

        //Stats
        public Dictionary<DinoStatTypeIndex, float> baseLevel;
        public Dictionary<DinoStatTypeIndex, float> increasePerWildLevel;
        public Dictionary<DinoStatTypeIndex, float> increasePerTamedLevel;
        public Dictionary<DinoStatTypeIndex, float> additiveTamingBonus; //Taming effectiveness
        public Dictionary<DinoStatTypeIndex, float> multiplicativeTamingBonus; //Taming effectiveness
    }

    public class ArkDinoEntryStatusComponent
    {
        [ArkConvertFlag("BaseFoodConsumptionRate", PropertyTypeIndex.FloatProperty, false, -0.01f)]
        public float baseFoodConsumptionRate { get; set; }
        [ArkConvertFlag("BabyDinoConsumingFoodRateMultiplier", PropertyTypeIndex.FloatProperty, false, 25.5f)]
        public float babyDinoConsumingFoodRateMultiplier { get; set; }
        [ArkConvertFlag("ExtraBabyDinoConsumingFoodRateMultiplier", PropertyTypeIndex.FloatProperty, false, 20.0f)]
        public float extraBabyDinoConsumingFoodRateMultiplier { get; set; }
        [ArkConvertFlag("FoodConsumptionMultiplier", PropertyTypeIndex.FloatProperty, false, 1f)]
        public float foodConsumptionMultiplier { get; set; }
    }

    public class ArkDinoFood
    {
        public string classname;
        public float foodEffectivenessMultiplier;
        public float affinityOverride;
        public float affinityEffectivenessMultiplier;
        public int foodCategory;
        public float priority;
    }

    /*public class ArkDinoStatsEntry
    {
        public float health;
        public float stamina;
        public float torpidity;
        public float oxygen;
        public float food;
        public float water;
        public float temperature;
        public float weight;
        public float meleeDamage;
        public float speed;
        public float temperatureFortitude;
        public float craftingSpeed;
    }*/
}
