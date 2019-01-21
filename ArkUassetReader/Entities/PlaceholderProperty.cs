using System;
using System.Collections.Generic;
using System.Text;

namespace ArkUassetReader.Entities
{
    /// <summary>
    /// Holds the place of an unfinished UProperty just to keep reading
    /// </summary>
    public class PlaceholderProperty : UProperty
    {
        public PlaceholderProperty(IOMemoryStream ms, UAssetFile f, bool isArray) : base(ms, f, isArray)
        {
            ms.position += length;
        }
    }
}
