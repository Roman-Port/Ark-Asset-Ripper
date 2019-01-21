using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkUassetReader.Entities
{
    public class UAssetFile : IDisposable
    {
        public static bool debug_mode_on = false;

        //Instance vars
        public string path;
        public string gamePath;
        public IOMemoryStream ms;
        public string[] name_table;
        public string classname;
        public Dictionary<string, string> packageInfo;
        public List<string> warnings = new List<string>();
        public List<GameObjectTableHead> ref_objects;
        public List<EmbeddedGameObjectTableHead> embeded_objects;

        //Private instance vars
        int nameTableLocation; //Located in header at 45
        int nameTableLength; //Located in header at 41
        int embeddedGameObjectCount; //Located in the header at 49
        int refGameObjectCount; //Located in the header at 57

        //API
        public static UProperty GetPropertyByNameStatic(List<UProperty> props, string name)
        {
            foreach(var p in props)
            {
                if (p.name == name)
                    return p;
            }
            return null;
        }

        public void Debug_PropLocationFinder()
        {
            while (true)
            {
                Console.WriteLine("Debug_PropLocationFinder awaiting command...");
                string s = Console.ReadLine();
                if (s == "")
                    break;
                int si = int.Parse(s);
                foreach(var p in embeded_objects)
                {
                    if (si >= p.dataLocation && si < p.dataLocation + p.dataLength)
                        p.WriteDebugString();
                }
            }
        }

        private static Dictionary<string, UAssetFile> fileCache = new Dictionary<string, UAssetFile>(); //File cache to ONLY BE USED FOR GETTING BLUEPRINT PROPS

        public List<UProperty> GetBlueprintProperties(bool throwOnUnreadable = true, bool workDown = true, bool useOurCache = false)
        {
            //Track down the property info we're going to read.
            EmbeddedGameObjectTableHead prop = null;
            foreach (EmbeddedGameObjectTableHead p in embeded_objects)
            {
                if (p.type.StartsWith("Default__"))
                    prop = p;
            }
            if (prop == null)
                throw new Exception("Could not find 'Default__' property!");

            List<UProperty> output = ReadBlueprintPropertiesAtFileLocation(prop.dataLocation, throwOnUnreadable);

            //If asked to work down, do so
            if (workDown && packageInfo != null)
            {
                UAssetFile workingFile = this;
                if (workingFile.packageInfo.ContainsKey("ParentClassPackage"))
                {
                    try
                    {
                        string nextFilePath = GetFullFilePathStatic(workingFile.packageInfo["ParentClassPackage"], Program.ARK_GAME_DIR);
                        if (fileCache.ContainsKey(nextFilePath))
                            workingFile = fileCache[nextFilePath];
                        else
                        {
                            workingFile = OpenFromFile(nextFilePath, Program.ARK_GAME_DIR);
                            fileCache.Add(nextFilePath, workingFile);
                        }
                       
                        output.AddRange(workingFile.GetBlueprintProperties(throwOnUnreadable, true));
                    } catch
                    {
                        //Stop
                    }
                } else
                {
                    //We have reached the end.
                }
            }

            return output;
        }

        public List<UProperty> ReadBlueprintPropertiesAtFileLocation(long pos, bool throwOnUnreadable = true)
        {
            //Jump to
            ms.position = pos;
            
            //Read until None type.
            List<string> warningsBuffer = new List<string>();
            List<UProperty> output = new List<UProperty>();
            while (true)
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    if (throwOnUnreadable)
                        throw ex;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was an unreadable property and reading has been halted. Properties before this point will read, but properties after this point will not.");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }
                }
                long startPos = ms.position;
                UProperty up = UProperty.ReadAnyProp(ms, this, out warningsBuffer);

                warnings.AddRange(warningsBuffer);
                foreach (string s in warningsBuffer)
                    Console.WriteLine(s);
                if (up == null)
                    break;
                output.Add(up);
                if (debug_mode_on)
                    Console.WriteLine($"{up.type} {up.name} after {startPos}, ending {ms.position}. U1:{up.unknown1}, U2:{up.unknown2}, U4: {up.index}, LEN:{up.length}");
            }
            return output;
        }

        public T GetPropertyByName<T>(string name, bool checkCase = true, bool throwOnFail = true)
        {
            //Get property standard
            UProperty uprop = GetPropertyByName(name, checkCase, throwOnFail);

            return (T)Convert.ChangeType(uprop, typeof(T));
        }

        public bool HasProperty(string name, bool checkCase = true, bool throwOnFail = true)
        {
            return GetPropertyByName(name, checkCase, throwOnFail) != null;
        }

        public UProperty GetPropertyByName(string name, bool checkCase = true, bool throwOnFail = true)
        {
            //Loop through properties
            var props = GetBlueprintProperties(throwOnFail);
            foreach(var p in props)
            {
                if(checkCase)
                {
                    if (p.name == name)
                        return p;
                } else
                {
                    if (p.name.ToLower() == name.ToLower())
                        return p;
                }
            }
            return null;
        }

        /// <summary>
        /// Get a ref object by property
        /// </summary>
        /// <param name="p"></param>
        /// <param name="workDown">If this is true, the program will attempt to follow the path of ids down.</param>
        /// <returns></returns>
        public GameObjectTableHead GetReferencedObject(UProperty p, bool workDown = false)
        {
            //Cast to object prop
            var objp = (Properties.ObjectProperty)p;

            //Get
            GameObjectTableHead r = GetReferencedObjectById(objp.objectIndex);

            if(workDown)
            {
                while (r.index != 0)
                    r = GetReferencedObjectById(r.index);
            }

            //Get ID
            return r;
        }

        /// <summary>
        /// Get a ref object by property
        /// </summary>
        /// <param name="p"></param>
        /// <param name="workDown">If this is true, the program will attempt to follow the path of ids down.</param>
        /// <returns></returns>
        public UAssetFile GetReferencedFile(UProperty p, bool workDown = false)
        {
            //Get package normally
            GameObjectTableHead h = GetReferencedObject(p, workDown);

            //Getpath
            string fullPath = GetFullFilePath(h);

            //Return loaded file
            return UAssetFile.OpenFromFile(fullPath, gamePath);
        }

        /// <summary>
        /// Get a ref object by ID
        /// </summary>
        /// <param name="p"></param>
        /// <param name="workDown">If this is true, the program will attempt to follow the path of ids down.</param>
        /// <returns></returns>
        public UAssetFile GetReferencedFileById(int id)
        {
            //Get package normally
            GameObjectTableHead h = GetReferencedObjectById(id);

            //Getpath
            string fullPath = GetFullFilePath(h);

            //Return loaded file
            return UAssetFile.OpenFromFile(fullPath, gamePath);
        }

        public static string GetFullFilePathStatic(GameObjectTableHead h, string gamePath)
        {
            //Get full path
            string relPath = h.name;

            return GetFullFilePathStatic(relPath, gamePath);
        }

        public static string GetFullFilePathStatic(string relPath, string gamePath)
        {
            //Trim
            if (relPath.StartsWith("/Game"))
                relPath = relPath.Substring("/Game".Length);

            //Append
            string fullPath = gamePath + relPath + ".uasset";
            return fullPath;
        }

        public static string GetGamePathFromFullPath(string absPath, string gamePath)
        {
            return "/Game/"+absPath.Substring(gamePath.Length).TrimStart('/').TrimStart('\\').Replace("\\", "/");
        }

        public string GetFullFilePath(GameObjectTableHead h)
        {
            return GetFullFilePathStatic(h, gamePath);
        }

        public GameObjectTableHead GetReferencedObjectById(int index)
        {
            int finalIndex;
            finalIndex = (int)MathF.Abs(index) - 1;

            //Get
            if (finalIndex >= ref_objects.Count)
                throw new Exception("Could not find referenced object! Maybe it is embedded?");
            GameObjectTableHead r = ref_objects[finalIndex];
            return r;
        }

        //Creation and private

        private static void DebugWrite(string msg)
        {
            if (debug_mode_on)
                Console.WriteLine(msg);
        }

        public static UAssetFile OpenFromFile(string path, string gamePath)
        {
            UAssetFile f;
            MemoryStream ms = new MemoryStream();
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                    fs.CopyTo(ms);
            } catch (IOException)
            {
                //The file is probably "in use" (usually it's not). Fall back to File.ReadAllBytes
                byte[] buf = File.ReadAllBytes(path);
                ms.Write(buf, 0, buf.Length);
            }
            ms.Position = 0;

            f = OpenFromStream(ms, gamePath);
            try
            {
                f.path = GetGamePathFromFullPath(path, gamePath);
            } catch
            {

            }
            return f;
        }

        /// <summary>
        /// ATTENTION: Input MemoryStream cannot be closed while this is still operating.
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static UAssetFile OpenFromStream(MemoryStream ms, string gamePath)
        {
            return OpenFromStream(new IOMemoryStream(ms, true), gamePath);
        }

        public static UAssetFile OpenFromStream(IOMemoryStream ms, string gamePath)
        {
            //Create object
            UAssetFile f = new UAssetFile();
            f.ms = ms;
            f.gamePath = gamePath;

            //Jump to the beginning of the file and read the header
            ms.position = 0;
            f.ReadUAssetHeader();

            //Read package info 
            f.ReadPackageInfoDict();

            //Grab our classname. Probably won't work in all cases
            try
            {
                string classname = f.packageInfo["GeneratedClass"];
                f.classname = classname.Split('.')[classname.Split('.').Length - 1].TrimEnd('\'');
            }
            catch { }

            //Jump to the beginning of the references array(?) and read the length
            f.ReadNameTable();

            //Read GameObjects
            f.ReadGameObjectHead();

            //Read embeded GameObjects
            f.ReadEmbeddedGameObjectTableHead();

            return f;
        }

        void ReadPackageInfoDict()
        {
            ms.position = packagePropertyDictLocation;

            //Read type
            List<string> unknown_type_array = ReadStringArray(ms);

            //Read the classname
            string classname = ms.ReadUEString();

            //Read one unknown ints
            int unknown1 = ms.ReadInt();

            //Read a list(?) of our classnames?
            List<string> unknownTable = ReadStringArray(ms);

            //Read an unknown string that seems to always say "Blueprint"
            string blueprintString = ms.ReadUEString();

            //Read the next unknown table
            unknownTable = ReadStringArray(ms);

            //Fixed length?
            for (int i = 0; i < 10; i++)
            {
                unknownTable.Add(ms.ReadUEString());
            }

            //Convert to dict
            packageInfo = new Dictionary<string, string>();
            for(int i = 0; i<unknownTable.Count / 2; i++)
            {
                int index = i * 2;
                if(!packageInfo.ContainsKey(unknownTable[index]))
                    packageInfo.Add(unknownTable[index], unknownTable[index + 1]);
            }
            
        }

        void ReadOtherTestSection()
        {
            //Read the section right after the game objects section.
            //Jump to
            ms.position = binaryIdTablePosition;

            //Keep reading structs
            List<int[]> output = new List<int[]>();
            while(ms.position < binaryIdTableEndPosition)
            {
                Console.WriteLine("==== NEW ====");
                //Read length
                int intCount = ms.ReadInt();

                //Read ints
                int[] outputInts = new int[intCount];
                for (int i = 0; i < intCount; i++)
                {
                    outputInts[i] = ms.ReadInt();
                    Console.WriteLine(outputInts[i]);
                }
                output.Add(outputInts);
                Console.WriteLine("---- END ----");
            }
            Console.WriteLine("Found " + output.Count);
            Console.ReadLine();
        }

        int binaryIdTablePosition;
        int binaryIdTableEndPosition;
        int packagePropertyDictLocation;

        void ReadUAssetHeader()
        {
            //Read through the header. Assume the cursor position is 0.
            //Skip the first few integers
            for (int i = 0; i < 7; i++)
                ms.ReadInt();

            //Skip the unknown string
            ms.ReadUEString();

            //Skip another integer
            ms.ReadInt();

            //Read the name table length
            nameTableLength = ms.ReadInt();

            //Read the name table position
            nameTableLocation = ms.ReadInt();

            //Read the length of embedded GameObjects
            embeddedGameObjectCount = ms.ReadInt();

            //Skip integer
            ms.ReadInt();

            //Read ref GameObject count
            refGameObjectCount = ms.ReadInt();

            //Skip one. We'll probably need this later. 61.
            ms.ReadInt();

            //Read position of the "binary id" table position
            binaryIdTablePosition = ms.ReadInt();

            //Skip integer
            ms.ReadInt();

            //Read end position
            binaryIdTableEndPosition = ms.ReadInt();

            //Read package property location
            packagePropertyDictLocation = ms.ReadInt();
        }

        List<string> ReadStringArray(IOMemoryStream ms)
        {
            int length = ms.ReadInt();
            List<string> output = new List<string>();
            for (int i = 0; i < length; i++)
                output.Add(ms.ReadUEString());
            return output;
        }

        void ReadGameObjectHead()
        {
            //Starts directly after the name table.
            ref_objects = new List<GameObjectTableHead>();
            for (int i = 0; i < refGameObjectCount; i++)
            {
                GameObjectTableHead h = GameObjectTableHead.ReadEntry(ms, this);
                ref_objects.Add(h);
                if(debug_mode_on)
                    Console.WriteLine($"GameObjectHead - coreType:{h.coreType}, objectType:{h.objectType}, name:{h.name}, u1:{h.unknown1}, u2:{h.unknown2}, u3:{h.index}, h4:{h.unknown4}");
            }
        }

        void ReadEmbeddedGameObjectTableHead()
        {
            //Read the unknown data directly after the GameObject head array.
            List<EmbeddedGameObjectTableHead> output = new List<EmbeddedGameObjectTableHead>();
            for (int i = 0; i < embeddedGameObjectCount; i++)
            {
                var o = EmbeddedGameObjectTableHead.ReadEntry(ms, this);
                if(debug_mode_on)
                    o.WriteDebugString();
                output.Add(o);
            }
            embeded_objects = output;
        }

        void ReadNameTable()
        {
            //Jump to the beginning of the name table.
            ms.position = nameTableLocation;
            //Read strings
            string[] output = new string[nameTableLength];
            for (int i = 0; i < nameTableLength; i++)
            {
                output[i] = ms.ReadUEString();
                if(debug_mode_on)
                    Console.WriteLine(i.ToString() + ":" + output[i]);
            }

            name_table = output;
        }

        public void Dispose()
        {
            //Close MemoryStream
            ms.ms.Close();
            ms.ms.Dispose();
        }
    }
}
