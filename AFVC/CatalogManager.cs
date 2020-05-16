using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Pastel;

namespace AFVC
{

    class CatalogManager
    {
        private readonly int OPTIONS = 5;
        public static readonly string fileLoc = "\\catalog.afv";
        public static readonly string inputFolder = "\\input";
        public static readonly string storage = "\\cards";

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
                    Console.WriteLine("0 - Update Catalog from Input\n1 - View Catalog\n2 - Add/Update\n3 - Delete\n4 - Close\n");
                    if(Int32.TryParse(Console.ReadLine(),out dec) && dec>=0 && dec<OPTIONS)
                        break;
                }

                switch (dec)
                {
                    case 0:
                        UpdateCatalogFromInput();
                        break;
                    case 1:
                        Console.WriteLine();
                        Console.WriteLine(catalog);
                        break;
                    case 2:
                        AddUpdateRecord();
                        break;
                    case 3:
                        Delete();
                        break;
                }
            } while (dec != OPTIONS-1);
        }

        private void Delete()
        {
            Console.WriteLine("Insert the code to "+"delete".Pastel(Color.Red));
            CatalogCode code = new CatalogCode(Console.ReadLine());
            catalog.Delete(code);
            Save();
        }

        private void AddUpdateRecord()
        {
            Console.WriteLine("Insert the new code to add");
            string input = Console.ReadLine();
            CatalogCode code = new CatalogCode(input);
            Console.WriteLine("Insert the title of this record");
            string title = Console.ReadLine();
            if (!catalog.Contains(code))
            {
                CreateFolderFor(code);
            }
            catalog.Update(code, title);
            Save();
        }

        private void CreateFolderFor(CatalogCode code)
        {
            Directory.CreateDirectory(folder + storage + FolderFor(code));
        }

        private string FolderFor(CatalogCode code)
        {
            if (code.Equals(CatalogCode.current))
                return "";
            return "\\" + string.Join("\\", code.CodePattern);
        }


        private void UpdateCatalogFromInput()
        {
            foreach (var pic in Directory.EnumerateFiles(folder + inputFolder))
            {
                CatalogCode code = new CatalogCode(Path.GetFileNameWithoutExtension(pic));
                CreateFolderFor(code);
                File.Move(pic, folder + storage + FolderFor(code)+"\\"+Path.GetFileName(pic));
                catalog.Update(code,"");
            }
            Save();
        }

        public static CatalogManager Setup(string path)
        {
            Directory.CreateDirectory(path + inputFolder);
            Directory.CreateDirectory(path + storage);
            File.Create(path + fileLoc).Close();
            return new CatalogManager(path);
        }

        private void Save()
        {
            File.Delete(folder+fileLoc);
            File.Create(folder + fileLoc).Close();
            File.AppendAllLines(folder+fileLoc,catalog.Serialize());
        }
    }
}
