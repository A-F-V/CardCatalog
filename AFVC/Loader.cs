using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AFVC
{
    internal class Loader
    {
        public static Catalog loaderFromFolder(string path)
        {
            string file = string.Concat(path, CatalogManager.fileLoc);
            string[] data = File.ReadAllLines(file);
            Dictionary<string, string> dict = data.ToDictionary(s => s.Split(',')[0], s => s.Split(',')[1]);
            return new Catalog(dict);
        }
    }
}