using ArkUassetReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.HighLevelEntities
{
    public class ArkWorldSettings
    {
        public Dictionary<DinoStatTypeIndex, float> additiveTamingBonusModifier; //TaM, 0.14
        public Dictionary<DinoStatTypeIndex, float> multiplicativeTamingBonusModifier; //TmM, 0.44
        public Dictionary<DinoStatTypeIndex, float> increasePerTamedLevelModifier; //IdM, 0.2

        public float increasePerWildLevelModifier = 1; //Almost always 1

        //Server config
        public float serverConfig_tamedBaseHealthMultiplier = 1;
        public float serverConfig_babyImprintingStatScaleMultiplier = 1;
    }
}
