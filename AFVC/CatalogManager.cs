using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ImageProcessor;
using ImageProcessor.Imaging;
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
        public static readonly string[] imageFormats = {".jpg", ".png", ".tif", ".bmp", ".gif", ".jpeg"};

        public static Color[] Colors =
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

        public static string[] tasks =
        {
            "Update Catalog From Input", "View Catalog", "Add/Update", "Shift", "Delete Folder", "Delete Card",
            "View Card(s)",
            "View Document",
            "Backup", "Move", "Search", "Open Folder", "Clear Console", "Close"
        };

        private static readonly int offdist = 3;
        private readonly Catalog catalog;


        private readonly string folder;
        private readonly int OPTIONS = tasks.Length;


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
                    for (var i = 0; i < tasks.Length; i++)
                        Console.WriteLine(
                            $"{i} - {tasks[i].Pastel(i % 2 == 0 ? Color.DeepSkyBlue : Color.DarkOrange)}");
                    if (int.TryParse(ReadAnswer(), out dec) && dec >= 0 && dec < OPTIONS)
                        break;
                }

                if (dec == 0 || dec == 4 || dec == 5 || dec == 8 || dec == 9)
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
                            PromptShift();
                            break;
                        case 4:
                            PromptDeleteFolder();
                            break;
                        case 5:
                            DeleteFile();
                            break;
                        case 6:
                            ViewCards();
                            break;
                        case 7:
                            PromptViewDocument();
                            break;
                        case 8:
                            PromptBackUp();
                            break;
                        case 9:
                            PromptMove();
                            break;
                        case 10:
                            PromptSearch();
                            break;
                        case 11:
                            PromptOpenFolder();
                            break;
                        case 12:
                            Console.Clear();
                            break;
                    }
                }
                catch (CatalogError e)
                {
                    Console.WriteLine("There was a serious error: " + e.Message.Pastel(Color.Red) + ". Reprompting...");
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    Console.WriteLine("There was a serious " + "error".Pastel(Color.Red) + ". Reprompting...");
                    Thread.Sleep(500);
                }
            } while (dec != OPTIONS - 1);
        }

        private void PromptShift()
        {
            Console.WriteLine("Please insert the range to shift along");
            var orig = new CodeRange(ReadAnswer());
            if (orig.fromCode.Equals(CatalogCode.current))
                throw new CatalogError("Cannot shift the root");
            Console.WriteLine("Please insert by how much");
            var dist = int.Parse(ReadAnswer());
            var shifted = orig + dist;
            if (catalog.Contains(shifted - orig, orig,
                (shifted - orig).fromCode.Youngest() - shifted.fromCode.Youngest()))
                throw new CatalogError($"Cannot shift {orig} by {dist}");
            MoveAndSave(orig, shifted);
        }

        private void PromptOpenFolder()
        {
            Console.WriteLine("Insert the code of the folder to open:");
            var code = new CatalogCode(ReadAnswer());
            if (!catalog.Contains(code))
                throw new CatalogError($"{code} does not exist");
            OpenFileProcess(folder + storage + FolderFor(code));
        }

        private void PromptViewDocument()
        {
            Console.WriteLine("Insert the code of the file to view:");
            var code = new CatalogCode(ReadAnswer());
            if (code.Equals(CatalogCode.current))
                throw new CatalogError("Cannot open root");
            if (!IsFile(code, out var path))
                throw new CatalogError($"{code} is not a file");
            OpenFileProcess(path);
        }

        private void PromptSearch()
        {
            Console.WriteLine("Insert the phrase to search:");
            var entries = catalog.Search(ReadAnswer());
            for (var x = 0; x < entries.Count; x++) Console.WriteLine($"{x}) {FancyEntryWithFileFlag(entries[x])}");

            if (entries.Count != 0)
            {

                Console.WriteLine("Would you like to " + "0) open ".Pastel(Color.Aqua) +
                                  "1) edit ".Pastel(Color.OrangeRed) + "2) view catalog from ".Pastel(Color.Aqua) +
                                  "3) back ".Pastel(Color.OrangeRed) +
                                  "one of the entries?");
                var dec = int.Parse(ReadAnswer());
                CatalogEntry e = null;
                if (dec <= 2 && dec >= 0)
                {
                    Console.WriteLine("Insert the number to view");
                    var num = int.Parse(ReadAnswer());
                    if (num < 0 || num >= entries.Count) throw new CatalogError($"Cannot aceess number {num}");
                    e = entries[num];
                }

                switch (dec)
                {
                    case 0:
                        if (IsFile(e.codePrefix, out var path))
                            OpenFileProcess(path);
                        else
                            OpenFileProcess(folder + storage + FolderFor(e.codePrefix));
                        break;
                    case 1:
                        AddUpdateRecord(e.codePrefix);
                        break;
                    case 2:
                        Console.WriteLine("And to what depth (-1 for all)?");
                        if (!int.TryParse(ReadAnswer(), out var depth)) depth = -1;
                        Console.WriteLine(TreePrint(e, depth));
                        break;
                }
            }
        }

        private void PromptMove()
        {
            Console.WriteLine("Please insert the original code");
            var from = new CatalogCode(ReadAnswer());
            Console.WriteLine("Please insert the new code");
            var to = new CatalogCode(ReadAnswer());
            if (!IsMoveConflict(from, to))
                MoveAndSave(from, to);
            else
                Console.WriteLine("Unable to rename");
        }

        private void Move(CatalogCode a, CatalogCode b)
        {
            if (!catalog.Contains(a))
                return;
            var entry = catalog.Get(a);
            var title = entry.name;
            foreach (var child in entry.children)
                Move(child.codePrefix, b + CatalogCode.Relative(a, child.codePrefix));
            if (!Directory.Exists(folder + storage + FolderFor(b)))
                Directory.CreateDirectory(folder + storage + FolderFor(b));
            if (IsFile(a, out var oldPath, out var x)) SetFileCode(oldPath, b);
            catalog.Update(b, title);
        }

        private void MoveAndSave(CatalogCode a, CatalogCode b)
        {
            if (!catalog.Contains(a))
                return;
            Move(a, b);
            catalog.Delete(a);
            DeleteFolderOfCode(a);
            Save(folder + fileLoc);
        }

        private void MoveAndSave(CodeRange a, CodeRange b)
        {
            if (a.Equals(b))
                return;
            while (true)
            {
                if (a.Span == 1)
                {
                    MoveAndSave(a.fromCode, b.fromCode);
                }
                else
                {
                    if (a.fromCode.Youngest() < b.fromCode.Youngest())
                    {
                        MoveAndSave(a.toCode, b.toCode);
                        a = new CodeRange(a.fromCode, a.toCode + -1);
                        b = new CodeRange(b.fromCode, b.toCode + -1);
                        continue;
                    }

                    MoveAndSave(a.fromCode, b.fromCode);
                    a = new CodeRange(a.fromCode + 1, a.toCode);
                    b = new CodeRange(b.fromCode + 1, b.toCode);
                    continue;
                }

                break;
            }
        }

        private bool IsMoveConflict(CatalogCode a, CatalogCode b)
        {
            return catalog.Contains(b);
        }

        private void PromptBackUp()
        {
            Console.WriteLine("Please insert where to backup to: ");
            var path = MorePaths.getFolderPath();
            if (path == null || path == folder)
                Console.WriteLine("Failed to back up");
            else
                BackUp(path);
        }

        private void BackUp(string newFolder)
        {
            Wipe(newFolder);
            Setup(newFolder);
            Save(newFolder + fileLoc);
            CopyFolder(folder + storage, newFolder + storage);
        }

        private void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            foreach (var entry in Directory.EnumerateFileSystemEntries(from))
            {
                var diff = entry.RemoveFirst(from.Length);
                if (Directory.Exists(entry))
                    CopyFolder(entry, to + diff);
                else
                    File.Copy(entry, to + diff);
            }
        }

        private void Wipe(string newFolder)
        {
            Directory.Delete(newFolder, true);
            Directory.CreateDirectory(newFolder);
        }

        private void DeleteFile()
        {
            Console.WriteLine("Insert the code to " + "delete".Pastel(Color.Red));
            var code = new CatalogCode(ReadAnswer());
            if (IsFile(code))
            {
                var path = Directory.EnumerateFiles(folder + storage + FolderFor(code))
                    .FirstOrDefault(s => s.Contains(code.ToString()));
                if (path != default)
                    File.Delete(path);
            }
        }

        private string FancyEntryWithFileFlag(CatalogEntry entry)
        {
            var isFile = IsFile(entry.codePrefix, out var path, out var isImage);
            return entry.FancifyEntry() + " " +
                   (isFile ? " ".PastelBg(isImage ? Color.DodgerBlue : Color.OrangeRed) : "");
        }

        private string TreePrint(CatalogEntry entry, int depth = -1, int offset = 0)
        {
            if (depth == 0)
                return "";
            var thisString = "|-" + FancyEntryWithFileFlag(entry) + "\n";
            if (depth > 1 || depth == -1)
                foreach (var child in entry.children)
                    thisString += new string(' ', offset + offdist) +
                                  TreePrint(child, depth == -1 ? -1 : depth - 1, offset + offdist);
            return thisString;
        }

        private void ViewCards()
        {
            if (File.Exists(folder + tempFile))
                File.Delete(folder + tempFolder + tempFile);

            Console.WriteLine("Insert the codes to " + "view".Pastel(Color.DeepSkyBlue));
            Console.WriteLine(
                "To create a range, use \"-\" between codes, use a \".\" at the end of codes to force not going down, and seperate groupings with a \",\"");
            var codeRanges = CodeRange.Parse(ReadAnswer());
            var cards = CardsInRange(codeRanges);
            if (cards.Count != 0)
            {
                foreach (var catalogCode in cards) Console.WriteLine(catalogCode.ToString().Pastel(Color.Aqua));

                Console.WriteLine();
                GenerateCardView(cards);
                OpenFileProcess(folder + tempFolder + tempFile);
            }
            else
            {
                Console.WriteLine("No card exists.");
            }
        }

        private List<CatalogCode> CardsInRange(List<CodeRange> codeRanges)
        {
            var output = new List<CatalogCode>();
            foreach (var codeRange in codeRanges)
            foreach (var child in catalog.Get(codeRange.fromCode.parent).children)
            {
                var temp = child.codePrefix;
                if (child.codePrefix.CompareTo(codeRange.fromCode) >= 0 &&
                    child.codePrefix.CompareTo(codeRange.toCode) <= 0)
                {
                    IsFile(temp, out var p, out var isImage);
                    if (isImage)
                        output.Add(temp);
                    if (codeRange.childrenAsWell)
                        output.AddRange(CardsOf(catalog.Get(temp)));
                }
            }

            return output;
        }

        private IEnumerable<CatalogCode> CardsOf(CatalogEntry temp)
        {
            var output = new List<CatalogCode>();
            foreach (var child in temp.children)
            {
                IsFile(child.codePrefix, out var p, out var isImage);
                if (isImage)
                    output.Add(child.codePrefix);
                output.AddRange(CardsOf(child));
            }

            return output;
        }

        private void GenerateCardView(List<CatalogCode> codes)
        {
            var output = new ImageFactory().Load(new Bitmap(1, 1));
            foreach (var code in codes)
            {
                var path = CardPath(code);
                var temp = new ImageLayer();
                temp.Image = Image.FromFile(path);
                temp.Position = new Point(0, output.Image.Height);
                var rl = new ResizeLayer(new Size(Math.Max(temp.Image.Width, output.Image.Width),
                    output.Image.Height + temp.Image.Height), ResizeMode.BoxPad, AnchorPosition.TopLeft);
                output.Resize(rl);
                output.Overlay(temp);
                temp.Dispose();
            }

            output.Image.Save(folder + tempFolder + tempFile, ImageFormat.Tiff);
            output.Dispose();
        }

        private void PrintCatalog()
        {
            Console.WriteLine("Insert the code you want to display (. for all):");
            var response = ReadAnswer();
            var code = new CatalogCode(response);

            Console.WriteLine("And to what depth (-1 for all)?");
            if (!int.TryParse(ReadAnswer(), out var depth)) depth = -1;
            if (code.Equals(CatalogCode.current) && depth >= 1)
                depth++;
            Console.WriteLine(TreePrint(catalog.Get(code), depth));
        }

        private bool IsFile(CatalogCode code, out string path, out bool b)
        {
            path = null;
            b = false;
            var testFolder = folder + storage + FolderFor(code);
            if (Directory.Exists(testFolder))
            {
                path = CardPath(code);
                b = imageFormats.Contains(Path.GetExtension(path));
                return path != default;
            }

            return false;
        }

        private bool IsFile(CatalogCode code, out string path)
        {
            return IsFile(code, out path, out var b);
        }

        private bool IsFile(CatalogCode code)
        {
            return IsFile(code, out var p);
        }

        private string CardPath(CatalogCode code)
        {
            var testFolder = folder + storage + FolderFor(code);
            return Directory.EnumerateFiles(testFolder)
                .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == code.ToString());
        }

        private void PromptDeleteFolder()
        {
            try
            {
                Console.WriteLine("Insert the code to " + "delete".Pastel(Color.Red));
                var code = new CatalogCode(ReadAnswer());
                catalog.Delete(code);
                DeleteFolderOfCode(code);
                Save(folder + fileLoc);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR".PastelBg(Color.Red) + "IN DELETING");
            }
        }

        private void DeleteFolderOfCode(CatalogCode code)
        {
            Directory.Delete(folder + storage + FolderFor(code), true);
        }

        private void AddUpdateRecord(CatalogCode from = null)
        {
            var code = from ?? CatalogCode.current + PromptCodeOrNewChild("Insert the new code to add");
            var title = PromptNewOrOldTitle(code);
            if (!catalog.Contains(code)) CreateFolderFor(code);
            catalog.Update(code, title);
            Save(folder + fileLoc);
        }

        private string PromptNewOrOldTitle(CatalogCode code)
        {
            var response = YNAnswer.No;
            var title = "";
            if (catalog.Contains(code))
            {
                title = catalog.Get(code).name;
                response = AskYNQuestion($"Would you like to keep the title {title.Pastel(Color.Aquamarine)}? (Y/N)");
            }

            if (response == YNAnswer.No)
            {
                Console.WriteLine($"Insert the title of {code.ToString().Pastel(Color.OrangeRed)}");
                title = ReadAnswer();
            }

            return title;
        }

        private YNAnswer AskYNQuestion(string s)
        {
            Console.WriteLine(s);
            var response = ReadAnswer();
            if (response.ToLower() == "y" || response.ToLower() == "yes")
                return YNAnswer.Yes;
            return YNAnswer.No;
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
            Console.Clear();
            PrintCatalog();
            foreach (var pic in Directory.EnumerateFiles(folder + inputFolder))
            {
                var p = PromptOpening(pic);
                var code = PromptCodeOrNewChild("Set the code for this file");
                var title = PromptNewOrOldTitle(code);

                CreateFolderFor(code);
                SetFileCode(pic, code);
                catalog.Update(code, title);
                p?.Kill();
            }

            Save(folder + fileLoc);
        }

        private CatalogCode PromptCodeOrNewChild(string promptMessage)
        {
            Console.WriteLine(promptMessage);
            var input = ReadAnswer();
            if (input == "")
                throw new CatalogError("Cannot alter the Root");

            if (input.EndsWith("."))
            {
                var code = new CatalogCode(input.RemoveLast(1));
                return catalog.NewChild(code);
            }

            return new CatalogCode(input);
        }

        private void SetFileCode(string originalFile, CatalogCode code)
        {
            var path = folder + storage + FolderFor(code) + "\\" + code + Path.GetExtension(originalFile);
            var temppath = Directory.EnumerateFiles(Path.GetDirectoryName(path))
                .FirstOrDefault(o => Path.GetFileNameWithoutExtension(o) == code.ToString());
            if (temppath != null)
                File.Delete(temppath);
            File.Move(originalFile, path);
        }

        private Process PromptOpening(string pic)
        {
            Process p = null;
            var response =
                AskYNQuestion($"Would you like to see {Path.GetFileName(pic).Pastel(Color.Aquamarine)}? (Y/N)");
            if (response == YNAnswer.Yes)
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

        private void Save(string path)
        {
            File.Delete(path);
            File.Create(path).Close();
            File.AppendAllLines(path, catalog.Serialize());
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
        internal bool childrenAsWell = true;
        internal CatalogCode fromCode;
        internal CatalogCode toCode;

        public int Span => toCode.Youngest() - fromCode.Youngest() + 1;

        public CodeRange(CatalogCode f, CatalogCode t, bool caw = true)
        {
            fromCode = f;
            toCode = t;
            childrenAsWell = caw;
        }

        public CodeRange(string range)
        {
            var codes = range.Split('-');
            var l1 = codes[0].Length;
            if (l1 >= 2 && codes[0][l1 - 1] == '.')
            {
                childrenAsWell = false;
                codes[0] = codes[0].RemoveLast(1);
            }

            fromCode = new CatalogCode(codes[0]);
            if (codes.Length == 1)
            {
                toCode = fromCode;
            }
            else
            {
                toCode = new CatalogCode(childrenAsWell ? codes[1] : codes[1].RemoveLast(1));
                if (!CatalogCode.SameFolder(fromCode, toCode) || fromCode.CompareTo(toCode) > 0)
                    throw new CatalogError("Invalid code range.");
            }
        }

        public static List<CodeRange> Parse(string readAnswer)
        {
            var output = new List<CodeRange>();
            foreach (var range in readAnswer.Split(',')) output.Add(new CodeRange(range));

            return output;
        }

        public static CodeRange operator +(CodeRange r, int diff)
        {
            return new CodeRange(r.fromCode + diff, r.toCode + diff, r.childrenAsWell);
        }

        public static CodeRange operator -(CodeRange a, CodeRange b)
        {
            if (a.Span != b.Span || !a.fromCode.parent.Equals(b.fromCode.parent) ||
                a.childrenAsWell != b.childrenAsWell)
                throw new CatalogError($"Cannot find difference between {a} and {b}");
            if (a.Equals(b))
                return null;
            CatalogCode temp;
            if (a.fromCode.Youngest() < b.fromCode.Youngest())
            {
                temp = a.fromCode.parent +
                       new CatalogCode(Math.Min(a.toCode.Youngest(), b.fromCode.Youngest() - 1).ToString());
                return new CodeRange(a.fromCode, temp);
            }

            temp = a.fromCode.parent +
                   new CatalogCode(Math.Max(a.fromCode.Youngest(), b.toCode.Youngest() + 1).ToString());
            return new CodeRange(temp, a.toCode);
        }

        public override bool Equals(object obj)
        {
            if (obj is CodeRange c)
                return c.toCode.Equals(toCode) && c.fromCode.Equals(fromCode) && c.childrenAsWell == childrenAsWell;
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"{fromCode} - {toCode}";
        }
    }
}