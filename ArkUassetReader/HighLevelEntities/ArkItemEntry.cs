using ArkUassetReader.ClassConverter;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.HighLevelEntities
{
    public class ArkItemEntry
    {
        public Tools.ArkImageAsset icon;
        public Tools.ArkImageAsset broken_icon;

        public string classname;
        public string blueprintPath;

        [ArkConvertFlag("bHideFromInventoryDisplay", Entities.PropertyTypeIndex.BoolProperty, false, false)]
        public bool hideFromInventoryDisplay { get; set; }

        [ArkConvertFlag("bUseItemDurability", Entities.PropertyTypeIndex.BoolProperty, false, false)]
        public bool useItemDurability { get; set; }

        [ArkConvertFlag("bTekItem", Entities.PropertyTypeIndex.BoolProperty, false, false)]
        public bool isTekItem { get; set; }

        [ArkConvertFlag("bAllowUseWhileRiding", Entities.PropertyTypeIndex.BoolProperty, false, false)]
        public bool allowUseWhileRiding { get; set; }

        [ArkConvertFlag("DescriptiveNameBase", Entities.PropertyTypeIndex.StrProperty, true, false)] //Required
        public string name { get; set; }

        [ArkConvertFlag("ItemDescription", Entities.PropertyTypeIndex.StrProperty, true, false)] //Required
        public string description { get; set; }

        [ArkConvertFlag("SpolingTime", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float spoilingTime { get; set; } //0 if not spoiling

        [ArkConvertFlag("BaseItemWeight", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float baseItemWeight { get; set; }

        [ArkConvertFlag("MinimumUseInterval", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float useCooldownTime { get; set; }

        [ArkConvertFlag("BaseCraftingXP", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float baseCraftingXP { get; set; }

        [ArkConvertFlag("BaseRepairingXP", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float baseRepairingXP { get; set; }

        [ArkConvertFlag("MaxItemQuantity", Entities.PropertyTypeIndex.IntProperty, false, 0)]
        public int maxItemQuantity { get; set; }

        //CONSUMABLES
        [ArkConvertFlag("Ingredient_WeightIncreasePerQuantity", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float increasePerQuanity_Weight { get; set; }

        [ArkConvertFlag("Ingredient_FoodIncreasePerQuantity", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float increasePerQuanity_Food { get; set; }

        [ArkConvertFlag("Ingredient_HealthIncreasePerQuantity", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float increasePerQuanity_Health { get; set; }

        [ArkConvertFlag("Ingredient_WaterIncreasePerQuantity", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float increasePerQuanity_Water { get; set; }

        [ArkConvertFlag("Ingredient_StaminaIncreasePerQuantity", Entities.PropertyTypeIndex.FloatProperty, false, 0)]
        public float increasePerQuanity_Stamina { get; set; }
    }
}
