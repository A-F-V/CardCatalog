using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    class Loader
    {
        public static Catalog loaderFromFolder(string path)
        {
            string file = String.Concat(path, CatalogManager.Extension);
            string[] data = File.ReadAllLines(file);
            Dictionary<string,string> dict = data.ToDictionary((s => s.Split(',')[0]), (s => s.Split(',')[1]));
            return new Catalog(dict);
        }
    }
}
