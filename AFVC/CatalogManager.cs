using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{

    class CatalogManager
    {
        private readonly int OPTIONS = 1;
        public static readonly string Extension = "\\cards.afv";

        private Catalog catalog;
        private string folder;

        public CatalogManager(Catalog catalog, string folder)
        {
            this.catalog = catalog;
            this.folder = folder;
        }

        public CatalogManager(string folder)
        {
            this.catalog = new Catalog();
            this.folder = folder;
        }


        public void Run()
        {
            int dec;
            do
            {
                while (true)
                {
                    Console.WriteLine("0 - Update Catalog from Storage\n");
                    if(Int32.TryParse(Console.ReadLine(),out dec) && dec>=0 && dec<OPTIONS)
                        break;
                }

                switch (dec)
                {
                    
                }
            } while (dec);
        }
    }
}
