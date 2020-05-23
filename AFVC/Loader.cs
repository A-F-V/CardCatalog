using System.IO;
using System.Linq;

namespace AFVC
{
    internal class Loader
    {
        public static Catalog loaderFromFolder(string path)
        {
            var file = string.Concat(path, CatalogManager.fileLoc);
            var data = File.ReadAllLines(file);
            var dict = data.ToDictionary(s => s.Split(',')[0], s => s.Split(',')[1]);
            return new Catalog(dict);
        }
    }
}