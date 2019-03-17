using ArkUassetReader.Entities.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string package;
        public Dictionary<string, string> packageInfo;
        public List<string> warnings = new List<string>();
        public List<GameObjectTableHead> ref_objects;
        public List<EmbeddedGameObjectTableHead> embeded_objects;
        public string file_path;

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

        public string GetParentPackage()
        {
            if (!packageInfo.ContainsKey("ParentClassPackage"))
                return null;
            return packageInfo["ParentClassPackage"];
        }

        public UAssetFile LoadParentPackage()
        {
            if (packageInfo.ContainsKey("ParentClassPackage"))
            {
                string nextFilePath = GetFullFilePathStatic(packageInfo["ParentClassPackage"], Program.ARK_GAME_DIR);
                return OpenFromFile(nextFilePath, Program.ARK_GAME_DIR);
            } else
            {
                return null;
            }
        }

        private static Dictionary<string, UAssetFile> fileCache = new Dictionary<string, UAssetFile>(); //File cache to ONLY BE USED FOR GETTING BLUEPRINT PROPS

        public List<UProperty> GetBlueprintProperties(bool throwOnUnreadable = true, bool workDown = true, bool useOurCache = false, bool readStructs = false)
        {
            //If we were asked to work down, find all of the files below this and read them.
            if (workDown)
                return WorkDownParentsProp(throwOnUnreadable, useOurCache, readStructs);

            //Otherwise, read the props for just this and return them.
            //Track down the property info we're going to read.
            EmbeddedGameObjectTableHead prop = null;
            foreach (EmbeddedGameObjectTableHead p in embeded_objects)
            {
                if (p.type.StartsWith("Default__"))
                    prop = p;
            }
            if (prop == null)
                throw new Exception("Could not find 'Default__' property!");

            List<UProperty> output = ReadBlueprintPropertiesAtFileLocation(prop.dataLocation, throwOnUnreadable, readStructs: readStructs);

            return output;
        }

        private List<UProperty> WorkDownParentsProp(bool throwOnUnreadable = true, bool useOurCache = false, bool readStructs = false)
        {
            List<UAssetFile> parents = new List<UAssetFile>();
            parents.Add(this);
            UAssetFile workingFile = this;
            while (workingFile != null)
            {
                if (!workingFile.packageInfo.ContainsKey("ParentClassPackage"))
                    break;
                string nextName = workingFile.packageInfo["ParentClassPackage"];
                if (nextName.StartsWith("/Script"))
                    break;
                string nextFilePath = GetFullFilePathStatic(nextName, Program.ARK_GAME_DIR);
                workingFile = OpenFromFile(nextFilePath, Program.ARK_GAME_DIR);
                parents.Add(workingFile);
            }

            //Now, work from the back of the parents stack forward
            List<UProperty> output = new List<UProperty>();
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                List<UProperty> props = parents[i].GetBlueprintProperties(throwOnUnreadable, false, useOurCache, readStructs);

                //Replace props that existed in the output
                foreach(UProperty p in props)
                {
                    if(p.type == "ArrayProperty")
                    {
                        //Add this to existing props if they exist
                        bool found = false;
                        foreach(UProperty pp in output)
                        {
                            if(pp.name == p.name && pp.index == p.index)
                            {
                                ArrayProperty app = (ArrayProperty)pp;
                                ArrayProperty ap = (ArrayProperty)p;
                                if(ap.items != null)
                                {
                                    if (app.items == null)
                                        app.items = new List<UProperty>();
                                    app.items.AddRange(ap.items);
                                }
                                found = true;
                            }
                        }
                        if(!found)
                            output.Add(p);
                    } else
                    {
                        var matchingProps = output.Where(x => x.name == p.name && x.index == p.index).ToArray();
                        foreach (var pp in matchingProps)
                            output.Remove(pp);
                        //Add this prop
                        output.Add(p);
                    }

                    
                }
            }

            return output;
        }

        /*public List<UProperty> GetBlueprintProperties(bool throwOnUnreadable = true, bool workDown = true, bool useOurCache = false, bool readStructs = false)
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

            List<UProperty> output = ReadBlueprintPropertiesAtFileLocation(prop.dataLocation, throwOnUnreadable, readStructs:readStructs);

            //If asked to work down, do so
            if (workDown && packageInfo != null)
            {
                UAssetFile workingFile = this;
                if (workingFile.packageInfo.ContainsKey("ParentClassPackage"))
                {
                    try
                    {
                        string nextFilePath = GetFullFilePathStatic(workingFile.packageInfo["ParentClassPackage"], Program.ARK_GAME_DIR);
                        workingFile = OpenFromFile(nextFilePath, Program.ARK_GAME_DIR);
                        //fileCache.Add(nextFilePath, workingFile);

                        List<UProperty> newProps = (workingFile.GetBlueprintProperties(throwOnUnreadable, true, readStructs: readStructs));
                        
                        //Add new props only
                        foreach(UProperty np in newProps)
                        {
                            if (output.Where(x => x.name == np.name).Count() == 0)
                                output.Add(np);
                            else
                            {
                                //Exists. However, if this is an array, we can add the items.
                                if(np.type == "ArrayProperty")
                                {
                                    //Find
                                    foreach(var npp in output)
                                    {
                                        if(npp.type == "ArrayProperty" && npp.name == np.name)
                                        {
                                            ((ArrayProperty) npp).items.AddRange(((ArrayProperty) np).items);
                                        }
}
                                }
                                
                            }
                        }
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
        }*/

        public List<UProperty> ReadBlueprintPropertiesAtFileLocation(long pos, bool throwOnUnreadable = true, bool readStructs = false)
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
                UProperty up = UProperty.ReadAnyProp(ms, this, out warningsBuffer, readStructs:readStructs);

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

        public T GetPropertyByName<T>(string name, bool checkCase = true, bool throwOnFail = true, bool openStructs = false)
        {
            //Get property standard
            UProperty uprop = GetPropertyByName(name, checkCase, throwOnFail, openStructs);

            return (T)Convert.ChangeType(uprop, typeof(T));
        }

        public bool HasProperty(string name, bool checkCase = true, bool throwOnFail = true, bool openStructs = false)
        {
            return GetPropertyByName(name, checkCase, throwOnFail, openStructs) != null;
        }

        public UProperty GetPropertyByName(string name, bool checkCase = true, bool throwOnFail = true, bool openStructs = false)
        {
            //Loop through properties
            var props = GetBlueprintProperties(throwOnFail, readStructs:openStructs);
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
            GameObjectTableHead r = objp.source.GetReferencedObjectById(objp.objectIndex);

            if(workDown)
            {
                while (r.index != 0)
                    r = objp.source.GetReferencedObjectById(r.index);
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
        public UAssetFile GetReferencedFile(UProperty p, bool workDown = true)
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
            string s =  "/Game/"+absPath.Substring(gamePath.Length).TrimStart('/').TrimStart('\\').Replace("\\", "/");
            if (s.EndsWith(".uasset"))
                return s.Substring(0, s.Length - ".uasset".Length);
            else
                return s;
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
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            f = OpenFromStream(fs, gamePath);
            try
            {
                f.path = GetGamePathFromFullPath(path, gamePath);
            } catch
            {

            }
            f.file_path = path;
            return f;
        }

        public static UAssetFile OpenFromFileInMemory(string path, string gamePath)
        {
            UAssetFile f;
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                fs.CopyTo(ms);
            ms.Position = 0;
            f = OpenFromStream(ms, gamePath);
            try
            {
                f.path = GetGamePathFromFullPath(path, gamePath);
            }
            catch
            {

            }
            f.file_path = path;
            return f;
        }

        /// <summary>
        /// ATTENTION: Input MemoryStream cannot be closed while this is still operating.
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static UAssetFile OpenFromStream(Stream ms, string gamePath)
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

            //Jump to the beginning of the references array(?) and read the length
            f.ReadNameTable();
            long afterNameTable = ms.position;

            //Read package info 
            f.ReadPackageInfoDict();

            //Grab our classname. Probably won't work in all cases
            try
            {
                string classname = f.packageInfo["GeneratedClass"];
                if (!classname.StartsWith("BlueprintGeneratedClass'"))
                    throw new Exception();
                f.package = classname.Split('.')[0].Substring("BlueprintGeneratedClass'".Length);
            }
            catch { }



            //Read GameObjects
            ms.position = afterNameTable;
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
            this.classname = ms.ReadUEString();

            //Read one unknown ints
            int unknown1 = ms.ReadInt();

            //Read a list(?) of our classnames?
            List<string> unknownTable = ReadStringArray(ms);

            //Read an unknown string that seems to always say "Blueprint"
            string blueprintString = ms.ReadUEString();

            //Read the next unknown table
            unknownTable = ReadStringArray(ms);

            //Apperently you read this until it is "None" on the name table. I don't think that's right...
            while(true)
            {
                ms.TryReadNameTableEntry(this, out string name);
                if (name == "None")
                    break;
                ms.position -= 4;
                string d = ms.ReadUEString();
                unknownTable.Add(d);
            }

            //Convert to dict
            packageInfo = new Dictionary<string, string>();
            for(int i = 0; i<unknownTable.Count / 2; i++)
            {
                int index = i * 2;
                if(!packageInfo.ContainsKey(unknownTable[index]))
                    packageInfo.Add(unknownTable[index], unknownTable[index + 1]);
                if (debug_mode_on)
                    Console.WriteLine($"[Package Info Dict] {unknownTable[index]}: {unknownTable[index + 1]}");
            }
            
        }

        void ReadOtherTestSection()
        {
            //Read the section right after the game objects section.
            //Jump to
            ms.position = binaryIdTablePosition;

            //Keep reading structs
            List<int[]> output = new List<int[]>();
            while(ms.position < thumbnailBeginPosition)
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
        int thumbnailBeginPosition;
        int packagePropertyDictLocation;

        void ReadUAssetHeader()
        {
            //Read through the header. Assume the cursor position is 0.
            //Skip the first few integers
            for (int i = 0; i < 7; i++)
                ms.ReadInt();

            //Skip the unknown string
            string u1 = ms.ReadUEString();

            //Skip another integer
            int u2 = ms.ReadInt();

            //Read the name table length
            nameTableLength = ms.ReadInt();

            //Read the name table position
            nameTableLocation = ms.ReadInt();

            //Read the length of embedded GameObjects
            embeddedGameObjectCount = ms.ReadInt();

            //Skip integer
            int u3= ms.ReadInt();

            //Read ref GameObject count
            refGameObjectCount = ms.ReadInt();

            //Skip one. We'll probably need this later. 61.
            int u4 = ms.ReadInt();

            //Read position of the "binary id" table position
            binaryIdTablePosition = ms.ReadInt();

            //Skip integer
            int u5 = ms.ReadInt();

            //Read end position
            thumbnailBeginPosition = ms.ReadInt();

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
                    Console.WriteLine($"GameObjectHead - index: {i}, coreType:{h.coreType}, objectType:{h.objectType}, name:{h.name}, u1:{h.unknown1}, u2:{h.unknown2}, u3:{h.index}, h4:{h.unknown4}");
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
                    Console.WriteLine("[Name Table] "+i.ToString() + ":" + output[i]);
            }

            name_table = output;
        }

        public byte[] GetThumbnailBytes()
        {
            //Jump to the position of the thumbnail
            ms.position = thumbnailBeginPosition;
            
            //Skip two unknown integers
            int u1 = ms.ReadInt();
            int u2 = ms.ReadInt();

            //Read length
            int length = ms.ReadInt();

            //Allocate a byte array and read it
            byte[] buf = ms.ReadBytes(length);
            return buf;
        }

        public void Dispose()
        {
            //Close MemoryStream
            ms.ms.Close();
            ms.ms.Dispose();
        }
    }
}
