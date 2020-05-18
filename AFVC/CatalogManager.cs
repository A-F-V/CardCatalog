using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Processors;
using Pastel;

namespace AFVC
{
    internal class CatalogManager
    {
        public static readonly string fileLoc = "\\catalog.afv";
        public static readonly string inputFolder = "\\input";
        public static readonly string storage = "\\cards";
        public static readonly string tempFolder = "\\temp";
        public static readonly string tempFile = "\\temp.tiff";

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
                            ViewCards();
                            break;
                    }
                }
                catch (CatalogError e)
                {
                    Console.WriteLine("There was a serious error: " + e.Message.Pastel(Color.Red) + ". Reprompting...");
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine("There was a serious " + "error".Pastel(Color.Red) + ". Reprompting...");
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

        private void ViewCards()
        {
            if(File.Exists(folder+tempFile))
                File.Delete(folder + tempFolder + tempFile);

            Console.WriteLine("Insert the codes to " + "view".Pastel(Color.DeepSkyBlue));
            Console.WriteLine("To create a range, use \"-\" between codes, and seperate groupings with a \",\"");
            List<CodeRange> codeRanges = CodeRange.Parse(ReadAnswer());
            List<CatalogCode> cards = CardsInRange(codeRanges);
            if (cards.Count != 0)
            {
                foreach (var catalogCode in cards)
                {
                    Console.WriteLine(catalogCode.ToString().Pastel(Color.Aqua));
                }

                Console.WriteLine();
                GenerateCardView(cards);
                OpenFileProcess(folder + tempFolder + tempFile);
            }
            else
                Console.WriteLine("No card exists.");
        }

        private List<CatalogCode> CardsInRange(List<CodeRange> codeRanges)
        {
            List<CatalogCode> output = new List<CatalogCode>();
            foreach (var codeRange in codeRanges)
            {
                foreach (var child in catalog.Get(codeRange.fromCode.parent).children)
                {
                    CatalogCode temp = child.codePrefix;
                    if (child.codePrefix.CompareTo(codeRange.fromCode) >= 0 &&
                        child.codePrefix.CompareTo(codeRange.toCode) <= 0)
                    {
                        output.AddRange(CardsOf(catalog.Get(temp)));
                        if (IsCard(temp))
                            output.Add(temp);
                    }
                }
            }
            return output;
        }

        private IEnumerable<CatalogCode> CardsOf(CatalogEntry temp)
        {
            List<CatalogCode> output = new List<CatalogCode>();
            foreach (var child in temp.children)
            {
                output.AddRange(CardsOf(child));
                if (IsCard(child.codePrefix))
                    output.Add(child.codePrefix);
            }

            return output;
        }

        private void GenerateCardView(List<CatalogCode> codes)
        {
            ImageFactory output = new ImageFactory().Load(new Bitmap(1, 1));
            foreach (var code in codes)
            {
                string path = CardPath(code);
                var temp = new ImageLayer();
                temp.Image = Image.FromFile(path);
                temp.Position = new Point(0,output.Image.Height);
                ResizeLayer rl = new ResizeLayer(new Size(Math.Max(temp.Image.Width, output.Image.Width),
                    output.Image.Height + temp.Image.Height),ResizeMode.Pad,AnchorPosition.TopLeft);
                output.Resize(rl);
                output.Overlay(temp);
            }
            output.Image.Save(folder+tempFolder+tempFile,ImageFormat.Tiff);
        }

        private void PrintCatalog()
        {
            Console.WriteLine(TreePrint(catalog.root));
        }

        private bool IsCard(CatalogCode code, out string path)
        {
            path = null;
            var testFolder = folder + storage + FolderFor(code);
            if (Directory.Exists(testFolder))
            {
                path = CardPath(code);
                return path != default;
            }
            return false;
        }

        private string CardPath(CatalogCode code)
        {
            string testFolder = folder + storage + FolderFor(code);
            return Directory.EnumerateFiles(testFolder)
                .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == code.ToString());
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


        private void UpdateCatalogFromInput() //TODO If already exist no need to set title
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
            var response = ReadAnswer();
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
            Directory.CreateDirectory(path + tempFolder);
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

    internal class CodeRange
    {
        internal CatalogCode fromCode;
        internal CatalogCode toCode;
        
        public CodeRange(string range)
        {
            string[] codes = range.Split('-');
            fromCode = new CatalogCode(codes[0]);
            if (codes.Length == 1)
                toCode = fromCode;
            else
            {
                toCode = new CatalogCode(codes[1]);
                if (!CatalogCode.SameFolder(fromCode, toCode) || fromCode.CompareTo(toCode) > 0)
                    throw new CatalogError("Invalid code range.");
            }
            
        }

        public static List<CodeRange> Parse(string readAnswer)
        {
            List<CodeRange> output = new List<CodeRange>();
            foreach (var range in readAnswer.Split(','))
            {
                output.Add(new CodeRange(range));
            }

            return output;
        }
    }
}