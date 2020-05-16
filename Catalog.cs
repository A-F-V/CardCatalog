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

        private class CatalogFolder :CatalogNode
        {
            private CatalogCode codepPrefix;
            List<CatalogNode>
        }

        private class CatalogRoot : CatalogEntry { }
    }

   
}
