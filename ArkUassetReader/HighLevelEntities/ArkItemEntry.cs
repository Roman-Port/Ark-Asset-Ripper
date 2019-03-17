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

        //CONSUMABLEs
        //UseItemAddCharacterStatusValues
        public Dictionary<string, ArkItemEntry_ConsumableAddStatusValue> addStatusValues = new Dictionary<string, ArkItemEntry_ConsumableAddStatusValue>();
    }

    public class ArkItemEntry_ConsumableAddStatusValue
    {
        [ArkConvertFlag("BaseAmountToAdd", Entities.PropertyTypeIndex.FloatProperty, true, null)] //Required
        public float baseAmountToAdd { get; set; }
        [ArkConvertFlag("bPercentOfMaxStatusValue", Entities.PropertyTypeIndex.BoolProperty, true, false)] //Required
        public bool percentOfMaxStatusValue { get; set; }
        [ArkConvertFlag("bPercentOfCurrentStatusValue", Entities.PropertyTypeIndex.BoolProperty, true, false)] //Required
        public bool percentOfCurrentStatusValue { get; set; }
        [ArkConvertFlag("bUseItemQuality", Entities.PropertyTypeIndex.BoolProperty, true, false)] //Required
        public bool useItemQuality { get; set; }
        [ArkConvertFlag("bAddOverTime", Entities.PropertyTypeIndex.BoolProperty, true, false)] //Required
        public bool addOverTime { get; set; }
        [ArkConvertFlag("bSetValue", Entities.PropertyTypeIndex.BoolProperty, true, false)] //Required
        public bool setValue { get; set; }
        [ArkConvertFlag("bSetAdditionalValue", Entities.PropertyTypeIndex.BoolProperty, true, false)] //Required
        public bool setAdditionalValue { get; set; }
        [ArkConvertFlag("AddOverTimeSpeed", Entities.PropertyTypeIndex.FloatProperty, true, false)] //Required
        public float addOverTimeSpeed { get; set; }
        [ArkConvertFlag("ItemQualityAddValueMultiplier", Entities.PropertyTypeIndex.FloatProperty, true, false)] //Required
        public float itemQualityAddValueMultiplier { get; set; }

        public string statusValueType; //Enum

    }
}
