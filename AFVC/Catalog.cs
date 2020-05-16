using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    class Catalog
    {
        private Dictionary<string, string> dict;

        public Catalog(Dictionary<string, string> dict)
        {
            this.dict = dict;
        }

        private class CatalogEntry
        {
            private CatalogCode codepPrefix;
        }

        private class CatalogFolder : CatalogEntry
        {
            private List<CatalogEntry> children;
        }

        private class CatalogFile : CatalogEntry { }
    }

   
}
