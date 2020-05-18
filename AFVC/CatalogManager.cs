using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Pastel;

namespace AFVC
{

    class CatalogManager
    {
        private readonly int OPTIONS = 6;
        public static readonly string fileLoc = "\\catalog.afv";
        public static readonly string inputFolder = "\\input";
        public static readonly string storage = "\\cards";
        public static readonly string photoviewer = "C:\\Program Files\\Windows Photo Viewer\\PhotoViewer.dll";
        public static string[] tasks = new string[]{"Update Catalog","View Catalog","Add/Update","Delete","View Card","Close"};
        private Catalog catalog;
        private string folder;
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


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
                    Console.WriteLine("0 - Update Catalog from Input\n1 - View Catalog\n2 - Add/Update\n3 - Delete\n4 - View Card\n5 - Close");
                    if(Int32.TryParse(ReadAnswer(), out dec) && dec>=0 && dec<OPTIONS)
                        break;
                }

                if (dec == 0 || dec == 2 || dec == 3)
                {
                    Console.WriteLine($"Do you want:\n1 - Back\n2 - {tasks[dec].Pastel(Color.GreenYellow)}");
                    int decC;
                    Int32.TryParse(ReadAnswer(), out decC);
                    if (decC != 2)
                        continue;
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
                    case 4:
                        ViewCard();
                        break;
                }
            } while (dec != OPTIONS-1);
        }

        private void ViewCard()
        {
            Console.WriteLine("Insert the code to " + "view".Pastel(Color.DeepSkyBlue));
            CatalogCode code = new CatalogCode(ReadAnswer());
            if (IsCard(code,out string path))
            {
                OpenFileProcess(path);
            }
            else
            {
                Console.WriteLine("No card exists.");
            }
        }

        private bool IsCard(CatalogCode code, out string path)
        {
            path = null;
            string testFolder = folder + storage + FolderFor(code);
            if (Directory.Exists(testFolder))
            {
                path = Directory.EnumerateFiles(testFolder)
                    .First(p => Path.GetFileNameWithoutExtension(p) == code.ToString());
                return path != null;
            }
            return false;
        }

        private void Delete()
        {
            try
            {
                Console.WriteLine("Insert the code to " + "delete".Pastel(Color.Red));
                CatalogCode code = new CatalogCode(ReadAnswer());
                catalog.Delete(code);
                Directory.Delete(folder + storage + FolderFor(code),true);
                Save();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR".PastelBg(Color.Red)+"IN DELETING");
                return;
            }
        }

        private void AddUpdateRecord()
        {
            Console.WriteLine("Insert the new code to add");
            string input = ReadAnswer();
            CatalogCode code = new CatalogCode(input);
            Console.WriteLine("Insert the title of this record");
            string title = ReadAnswer();
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

                var p = OpenFileProcess(pic);
                Console.WriteLine("Set the code for this file");
                CatalogCode code = new CatalogCode(ReadAnswer());
                Console.WriteLine("Set title for this file");
                string title = ReadAnswer();
                CreateFolderFor(code);
                File.Move(pic, folder + storage + FolderFor(code)+"\\"+code.ToString()+Path.GetExtension(pic));
                catalog.Update(code,title);
                p?.Kill();
            }
            Save();
        }

        private static Process OpenFileProcess(string pic)
        {
            Process p = Process.Start(pic);
            Thread.Sleep(500);
            Process thisP = Process.GetCurrentProcess();
            IntPtr s = thisP.MainWindowHandle;
            SetForegroundWindow(s);
            return p;
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

        private string ReadAnswer()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            string output = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            return output;
        }
    }
}
