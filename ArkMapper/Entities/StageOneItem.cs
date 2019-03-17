using System;
using System.Collections.Generic;
using System.Text;

namespace ArkMapper.Entities
{
    public class StageOneItem
    {
        public string classname;
        public string parentPackagePath;
        public string packagePath;
        public string filename;

        public List<StageOneItem> children;
    }
}
