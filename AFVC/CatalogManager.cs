using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Pastel;

namespace AFVC
{
    internal class CatalogManager
    {
        public static readonly string fileLoc = "\\catalog.afv";
        public static readonly string inputFolder = "\\input";
        public static readonly string storage = "\\cards";
        public static readonly string photoviewer = "C:\\Program Files\\Windows Photo Viewer\\PhotoViewer.dll";

        public static string[] tasks =
        {
            "Update Catalog", "View Catalog", "Add/Update", "Delete Folder", "Delete Card", "View Card(s)",
            "Close"
        };

        private static readonly int offdist = 3;
        private readonly int OPTIONS = tasks.Length;
        private readonly Catalog catalog;

        private readonly Color[] colors =
        {
            Color.Crimson,
            Color.FromArgb(213, 254, 119),
            Color.FromArgb(57, 240, 119),
            Color.FromArgb(0, 201, 167),
            Color.MediumPurple,
            Color.DarkMagenta,
            Color.Fuchsia,
            Color.Gold
        };

        private readonly string folder;


        public CatalogManager(Catalog catalog, string folder)
        {
            this.catalog = catalog;
            this.folder = folder;
        }

        public CatalogManager(string folder)
        {
            catalog = new Catalog();
            this.folder = folder;
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        public void Run()
        {
            int dec;
            do
            {
                while (true)
                {
                    Console.WriteLine(
                        "0 - Update Catalog from Input\n1 - View Catalog\n2 - Add/Update\n3 - Delete Folder\n4 - Delete Card\n5 - View Card(s)\n6 - Close");
                    if (int.TryParse(ReadAnswer(), out dec) && dec >= 0 && dec < OPTIONS)
                        break;
                }

                if (dec == 0 || dec == 2 || dec == 3 || dec == 4 || dec == 6)
                {
                    Console.WriteLine($"Do you want:\n1 - Back\n2 - {tasks[dec].Pastel(Color.GreenYellow)}");
                    int decC;
                    int.TryParse(ReadAnswer(), out decC);
                    if (decC != 2)
                        continue;
                }

                try
                {
                    switch (dec)
                    {
                        case 0:
                            UpdateCatalogFromInput();
                            break;
                        case 1:
                            Console.WriteLine();
                            PrintCatalog();
                            break;
                        case 2:
                            AddUpdateRecord();
                            break;
                        case 3:
                            DeleteFolder();
                            break;
                        case 4:
                            DeleteCard();
                            break;
                        case 5:
                            ViewCard();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("There was a serious " + "error".Pastel(Color.Red)+ ". Reprompting...");
                    Thread.Sleep(1000);
                }
            } while (dec != OPTIONS - 1);
        }

        private void DeleteCard()
        {
            Console.WriteLine("Insert the code to " + "delete".Pastel(Color.Red));
            var code = new CatalogCode(ReadAnswer());
            if (IsCard(code))
            {
                var path = Directory.EnumerateFiles(folder + storage + FolderFor(code))
                    .FirstOrDefault(s => s.Contains(code.ToString()));
                if (path != default)
                    File.Delete(path);
            }
        }

        private string TreePrint(CatalogEntry entry, int offset = 0)
        {
            var thisString = "|-" +
                             $"{entry.codePrefix.ToString().Pastel(Color.OrangeRed)}. {(entry.name == null ? string.Empty : entry.name.Pastel(colors[Math.Min(entry.codePrefix.Depth - 1, colors.Length - 1)]))} " +
                             (IsCard(entry.codePrefix) ? " ".PastelBg(Color.Azure) : "") + "\n";
            foreach (var child in entry.children)
                thisString += new string(' ', offset + offdist) + TreePrint(child, offset + offdist);
            return thisString;
        }

        private void PrintCatalog()
        {
            Console.WriteLine(TreePrint(catalog.root));
        }

        private void ViewCard()
        {
            Console.WriteLine("Insert the code to " + "view".Pastel(Color.DeepSkyBlue));
            var code = new CatalogCode(ReadAnswer());
            if (IsCard(code, out var path))
                OpenFileProcess(path);
            else
                Console.WriteLine("No card exists.");
        }

        private bool IsCard(CatalogCode code, out string path)
        {
            path = null;
            var testFolder = folder + storage + FolderFor(code);
            if (Directory.Exists(testFolder))
            {
                path = Directory.EnumerateFiles(testFolder)
                    .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == code.ToString());
                return path != default;
            }

            return false;
        }

        private bool IsCard(CatalogCode code)
        {
            return IsCard(code, out var p);
        }

        private void DeleteFolder()
        {
            try
            {
                Console.WriteLine("Insert the code to " + "delete".Pastel(Color.Red));
                var code = new CatalogCode(ReadAnswer());
                catalog.Delete(code);
                Directory.Delete(folder + storage + FolderFor(code), true);
                Save();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR".PastelBg(Color.Red) + "IN DELETING");
            }
        }

        private void AddUpdateRecord()
        {
            Console.WriteLine("Insert the new code to add");
            var input = ReadAnswer();
            var code = new CatalogCode(input);
            Console.WriteLine("Insert the title of this record");
            var title = ReadAnswer();
            if (!catalog.Contains(code)) CreateFolderFor(code);
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
                var p = PromptOpening(pic);
                Console.WriteLine("Set the code for this file");
                var code = new CatalogCode(ReadAnswer());
                Console.WriteLine("Set title for this file");
                var title = ReadAnswer();
                CreateFolderFor(code);
                var path = folder + storage + FolderFor(code) + "\\" + code + Path.GetExtension(pic);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(pic, path);
                catalog.Update(code, title);
                p?.Kill();
            }

            Save();
        }

        private Process PromptOpening(string pic)
        {
            Console.WriteLine($"Would you like to see {Path.GetFileName(pic).Pastel(Color.Aquamarine)}? (Y/N)");
            var response = Console.ReadLine();
            Process p = null;
            if (response.ToLower() == "y" || response.ToLower() == "yes")
                p = OpenFileProcess(pic);
            return p;
        }

        private static Process OpenFileProcess(string pic)
        {
            var p = Process.Start(pic);
            var thisP = Process.GetCurrentProcess();
            var s = thisP.MainWindowHandle;
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
            File.Delete(folder + fileLoc);
            File.Create(folder + fileLoc).Close();
            File.AppendAllLines(folder + fileLoc, catalog.Serialize());
        }

        private string ReadAnswer()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            var output = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            return output;
        }
    }
}