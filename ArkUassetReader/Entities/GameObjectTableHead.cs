using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities
{
    /// <summary>
    /// Entry in the game object table directly after the name table
    /// </summary>
    public class GameObjectTableHead
    {
        public string coreType; //Seems to be script or UObject so far
        public int unknown1;
        public string objectType; //Class sometimes, other when coreType is script
        public int unknown2;
        public int index; //Index
        public string name; //Name used by the game
        public int unknown4;

        public static GameObjectTableHead ReadEntry(IOMemoryStream ms, UAssetFile f)
        {
            //Read in
            GameObjectTableHead g = new GameObjectTableHead();
            g.coreType = ms.ReadNameTableEntry(f);
            g.unknown1 = ms.ReadInt();
            g.objectType = ms.ReadNameTableEntry(f);
            g.unknown2 = ms.ReadInt();
            g.index = ms.ReadInt();
            g.name = ms.ReadNameTableEntry(f);
            g.unknown4 = ms.ReadInt();
            return g;
        }
    }
}
