using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities
{
    /// <summary>
    /// Objects directly after the GameObject table.
    /// </summary>
    public class EmbeddedGameObjectTableHead
    {
        public long entryLocation; //Location of this entry.

        public int id;
        public int unknown2;
        public int unknown3;
        public string type;
        public int unknown4;
        public int unknown5;
        public int dataLength;
        public int dataLocation; //Location of the data from the beginning of the file
        public int unknown6;
        public int unknown7;
        public int unknown8;
        public int unknown9;
        public int unknown10;
        public int unknown11;
        public int unknown12;
        public int unknown13;
        public int unknown14;

        public static EmbeddedGameObjectTableHead ReadEntry(IOMemoryStream ms, UAssetFile f)
        {
            //Read in
            EmbeddedGameObjectTableHead g = new EmbeddedGameObjectTableHead();
            g.entryLocation = ms.position;
            g.id = ms.ReadInt();
            g.unknown2 = ms.ReadInt();
            g.unknown3 = ms.ReadInt();
            g.type = ms.ReadNameTableEntry(f);
            g.unknown4 = ms.ReadInt();
            g.unknown5 = ms.ReadInt();
            g.dataLength = ms.ReadInt();
            g.dataLocation = ms.ReadInt();
            g.unknown6 = ms.ReadInt();
            g.unknown7 = ms.ReadInt();
            g.unknown8 = ms.ReadInt();
            g.unknown9 = ms.ReadInt();
            g.unknown10 = ms.ReadInt();
            g.unknown11 = ms.ReadInt();
            g.unknown12 = ms.ReadInt();
            g.unknown13 = ms.ReadInt();
            g.unknown14 = ms.ReadInt();
            return g;
        }

        public void WriteDebugString()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"DataPosition: {dataLocation}, ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"DataLength: {dataLength}, ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Type: {type}, ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"EntryLocation: {entryLocation}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n");
        }
    }
}
