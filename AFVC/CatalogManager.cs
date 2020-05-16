using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{

    class CatalogManager
    {
       public static readonly string Extension = "\\cards.afv";
        private Catalog catalog;
        private string folder;

        public CatalogManager(Catalog catalog, string folder)
        {
            this.catalog = catalog;
            this.folder = folder;
        }


    }
}
