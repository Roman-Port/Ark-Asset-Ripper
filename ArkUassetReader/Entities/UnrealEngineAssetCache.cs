using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkUassetReader.Entities
{
    public class UnrealEngineAssetCache
    {
        //Public
        public Dictionary<string, string> path_table;
        public string gameRootDir;

        //Api
        public string GetFullPathByKey(string k)
        {
            //Find path
            string relPath = path_table[k];

            //Append
            return gameRootDir + relPath;
        }

        //Creation
        public static UnrealEngineAssetCache OpenFromFile(string path, string gameRootDir)
        {
            UnrealEngineAssetCache f;
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(path, FileMode.Open))
                fs.CopyTo(ms);
            ms.Position = 0;

            f = OpenFromStream(ms, gameRootDir);
            return f;
        }

        public static UnrealEngineAssetCache OpenFromStream(MemoryStream ms, string gameRootDir)
        {
            return OpenFromStream(new IOMemoryStream(ms, true), gameRootDir);
        }

        public static UnrealEngineAssetCache OpenFromStream(IOMemoryStream ms, string gameRootDir)
        {
            //Create object
            UnrealEngineAssetCache f = new UnrealEngineAssetCache();
            f.gameRootDir = gameRootDir.TrimEnd('/');

            //Jump to location that gives us the address of the table.
            ms.position = 8;
            int tablePos = ms.ReadInt();

            //Jump to table
            ms.position = tablePos;

            //Begin reading in dictonary.
            f.path_table = new Dictionary<string, string>();

            //Read length of the table
            int tableLen = ms.ReadInt();

            //Read table in
            for(int i = 0; i<tableLen; i++)
            {
                string value = ms.ReadUEString();
                string key = ms.ReadUEString();
                if(value != "" && key != "")
                    f.path_table.Add(key, value);
            }

            return f;
        }
    }
}
