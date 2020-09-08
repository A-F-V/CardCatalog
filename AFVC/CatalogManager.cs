using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using ImageProcessor;
using ImageProcessor.Imaging;
using Pastel;
using Image = System.Drawing.Image;

namespace AFVC
{
    internal class CatalogManager
    {
        public static readonly string fileLoc = "\\catalog.afv";
        public static readonly string inputFolder = "\\input";
        public static readonly string outputFolder = "\\output";
        public static readonly string storage = "\\cards";
        public static readonly string tempFolder = "\\temp";
        public static readonly string tempFile = "\\temp.tiff";
        public static readonly string[] imageFormats = {".jpg", ".png", ".tif", ".bmp", ".gif", ".jpeg"};
        static PastelConsole PC = new PastelConsole(ColourPalette.MarineFields);
        public static string[] tasks =
        {
            "Update Catalog From Input", "View Catalog", "Add/Update", "Shift", "Delete Folder", "Delete Card",
            "View Card(s)", "View Document", "Backup", "Move", "Search", "Open Folder", "Clear Console","Clean Folders",
            "Generate Table of Contents",
            "Close"
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
                    for (int i = 0; i < tasks.Length; i++)
                        PC.FormatWriteLine(
                            $"{{{(i % 2 == 0 ? 2 : -2)}}} - {{{(i%2==0?2:-2)}}}",i,tasks[i]);
                    if (int.TryParse(ReadAnswer(), out dec) && dec >= 0 && dec < OPTIONS)
                        break;
                }

                if (dec == 0 || dec == 4 || dec == 5 || dec == 8 || dec == 9)
                {
                    PC.FormatWriteLine("Do you want:\n1 - Back\n2 - {2}",tasks[dec]);
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
                            PromptUpdateCatalogFromInput();
                            break;
                        case 1:
                            Console.WriteLine();
                            PrintCatalog();
                            break;
                        case 2:
                            PromptAddUpdateRecord();
                            break;
                        case 3:
                            PromptShift();
                            break;
                        case 4:
                            PromptDeleteFolder();
                            break;
                        case 5:
                            PromptDeleteFile();
                            break;
                        case 6:
                            PromptViewCards();
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
                        case 13:
                            CleanFolders(folder+storage);
                            break;
                        case 14:
                            GenerateTableOfContents();
                            break;
                    }
                }
                catch (Exception e)
                {
                    PC.FormatWriteLine("There was a serious error: {-3} Reprompting...", e.Message);
                    Thread.Sleep(500);
                }
            } while (dec != OPTIONS - 1);
        }

        private void GenerateTableOfContents()
        {
            Save(folder+outputFolder+"\\ToC.txt",NonTreePrint(catalog.root));
        }

        private void CleanFolders(string path) //CLEANS CHILDREN NOT SELF
        {
            string[] subDirectories = Directory.GetDirectories(path);
            foreach (string dir in subDirectories)
            {
                CleanFolders(dir);
                string name = Path.GetFileName(dir);
                if (name.Contains(" ("))
                {
                    string newName = name.Split('(')[0].RemoveLast(1);
                    Directory.Move(dir, path + "\\" + newName);
                }
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (name.Contains(" ("))
                {
                    string newName = name.Split('(')[0].RemoveLast(1);
                    Directory.Move(file, path + "\\" + newName+Path.GetExtension(file));
                }
            }
        }

        private void PromptShift()
        {
            PC.WriteLine("Please insert the range to shift along:");
            CodeRange orig = new CodeRange(ReadAnswer());
            if (orig.fromCode.Equals(CatalogCode.current))
                throw new CatalogError("Cannot shift the root");
            PC.WriteLine("Please insert by how much?");
            int dist = int.Parse(ReadAnswer());
            if (dist == 0)
                return;
            CodeRange shifted = orig + dist;
            if (catalog.Contains(shifted - orig, orig,
                (shifted - orig).fromCode.Youngest() - shifted.fromCode.Youngest()))
                throw new CatalogError($"Cannot shift {orig} by {dist}");
            MoveAndSave(orig, shifted);
        }

        private void PromptOpenFolder()
        {
            PC.WriteLine("Insert the code of the folder to open:");
            CatalogCode code = new CatalogCode(ReadAnswer());
            if (!catalog.Contains(code))
                throw new CatalogError($"{code} does not exist");
            OpenFileProcess(folder + storage + FolderFor(code));
        }

        private void PromptViewDocument()
        {
            PC.WriteLine("Insert the code of the file to view:");
            CatalogCode code = new CatalogCode(ReadAnswer());
            if (code.Equals(CatalogCode.current))
                throw new CatalogError("Cannot open root");
            if (!IsFile(code, out string path))
                throw new CatalogError($"{code} is not a file");
            OpenFileProcess(path);
        }

        private void PromptSearch()
        {
            PC.WriteLine("Insert the phrase to search:");
            List<CatalogEntry> entries = catalog.Search(ReadAnswer());
            for (int x = 0; x < entries.Count; x++)
            {
                PC.FormatWrite("{-2}) ",x);
                Console.WriteLine(FancyEntryWithFileFlag(entries[x]));
            }

            if (entries.Count != 0)
            {
                PC.FormatWriteLine("Would you like to {3} {-3} {3} {-3} one of the entries? ", "0) open","1) edit ", "2) view catalog from ","3) back ");
                int dec = int.Parse(ReadAnswer());
                CatalogEntry e = null;
                if (dec <= 2 && dec >= 0)
                {
                    PC.WriteLine("Insert the number to view");
                    int num = int.Parse(ReadAnswer());
                    if (num < 0 || num >= entries.Count) throw new CatalogError($"Cannot aceess number {num}");
                    e = entries[num];
                }

                switch (dec)
                {
                    case 0:
                        if (IsFile(e.codePrefix, out string path))
                            OpenFileProcess(path);
                        else
                            OpenFileProcess(folder + storage + FolderFor(e.codePrefix));
                        break;
                    case 1:
                        PromptAddUpdateRecord(e.codePrefix);
                        break;
                    case 2:
                        PC.WriteLine("And to what depth (-1 for all)?");
                        if (!int.TryParse(ReadAnswer(), out int depth)) depth = -1;
                        Console.WriteLine(TreePrint(e, depth));
                        break;
                }
            }
        }

        private void PromptMove()
        {
            CatalogCode from = PromptCodeOrNewChild("Please insert the original code");
            CatalogCode to = PromptCodeOrNewChild("Please insert the new code");
            if (!IsMoveConflict(from, to))
                MoveAndSave(from, to);
            else
                PC.FormatWriteLine("Unable to rename {-3} to {-3}",from,to);
        }

        private void Move(CatalogCode a, CatalogCode b)
        {
            if (!catalog.Contains(a))
                return;
            CatalogEntry entry = catalog.Get(a);
            string title = entry.name;
            foreach (CatalogEntry child in entry.children)
                Move(child.codePrefix, b + CatalogCode.Relative(a, child.codePrefix));
            if (!Directory.Exists(folder + storage + FolderFor(b)))
                Directory.CreateDirectory(folder + storage + FolderFor(b));
            if (IsFile(a, out string oldPath, out bool x))
                SetFileCode(oldPath, b,title);
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
            return !catalog.Contains(a) || catalog.Contains(b);
        }

        private void PromptBackUp()
        {
            PC.WriteLine("Please insert where to backup to: ");
            string path = MorePaths.getFolderPath();
            if (path == null || path == folder || path.Contains(folder))
                PC.FormatWriteLine("Failed to back up to {0}",path);
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
            foreach (string entry in Directory.EnumerateFileSystemEntries(from))
            {
                string diff = entry.RemoveFirst(from.Length);
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

        private void PromptDeleteFile()
        {
            PC.FormatWriteLine("Insert the code to {-3}", "delete");
            CatalogCode code = new CatalogCode(ReadAnswer());
            if (IsFile(code))
            {
                YNAnswer response =
                    AskYNQuestion($"Are you sure you want to delete {catalog.Get(code).FancifyEntry()}?");
                if (response == YNAnswer.Yes)
                {
                    string path = Directory.EnumerateFiles(folder + storage + FolderFor(code))
                        .FirstOrDefault(s => s.Contains(code.ToString()));
                    if (path != default)
                        File.Delete(path);
                }
            }
        }

        private string FancyEntryWithFileFlag(CatalogEntry entry)
        {
            bool isFile = IsFile(entry.codePrefix, out string path, out bool isImage);
            //Color bg = isFile ? (isImage ? Color.DodgerBlue : Color.OrangeRed) : Color.FromArgb(12, 12, 12);
            return entry.FancifyEntry(isFile,-3) +
                   (isFile ? PC.Format($"{{{(isImage ? 0 : -2)}}}", " <\u25A0>") : "");
        }

        private string TreePrint(CatalogEntry entry, int depth = -1, int spacing = 3, string head = "",string body = "") //TODO LIKE CONSOLE TREE
        {
            if (depth == 0)
                return "";
            string thisString = head + FancyEntryWithFileFlag(entry) + "\n";
            if (depth > 1 || depth == -1)
                for (int i = 0; i < entry.children.Count; i++)
                {
                    CatalogEntry child = entry.children[i];
                    string childheader =  (i == entry.children.Count - 1 ? "\u2514" : "\u251C")+new String('\u2500',spacing);
                    string childbody = (i == entry.children.Count - 1? " " :"\u2502") + new String(' ',spacing);
                    thisString += TreePrint(child, depth == -1 ? -1 : depth - 1, spacing, body+childheader,body+childbody);
                }
            return thisString;
        }

        private string NonTreePrint(CatalogEntry entry, int depth = -1)
        {
            if(depth == 0)
                return "";
            bool isFile = IsFile(entry.codePrefix, out string path, out bool isImage);
            string thisString = new String('\t', entry.codePrefix.Depth) +entry.FileName+
                                (isFile ?  " <\u25A0>":"")+"\n";
            if (depth > 1 || depth == -1)
                for (int i = 0; i < entry.children.Count; i++)
                {
                    CatalogEntry child = entry.children[i];
                    thisString += NonTreePrint(child, depth == -1 ? -1 : depth - 1);
                }
            return thisString;
        }

        private void PromptViewCards()
        {
            if (File.Exists(folder + tempFile))
                File.Delete(folder + tempFolder + tempFile);

            PC.FormatWriteLine("Insert the codes to {0}","view");
            PC.WriteLine(
                "To create a range, use \"-\" between codes, use a \".\" at the end of codes to force not going down, and seperate groupings with a \",\"");
            List<CodeRange> codeRanges = CodeRange.Parse(ReadAnswer());
            List<CatalogCode> cards = CardsInRange(codeRanges);
            if (cards.Count != 0)
            {
                foreach (CatalogCode catalogCode in cards) PC.FormatWriteLine("{0}",catalog.Get(catalogCode).FileName);

                Console.WriteLine();
                GenerateCardView(cards);
                OpenFileProcess(folder + tempFolder + tempFile);
            }
            else
            {
                PC.WriteLine("No card exists.");
            }
        }

        private List<CatalogCode> CardsInRange(List<CodeRange> codeRanges)
        {
            List<CatalogCode> output = new List<CatalogCode>();
            foreach (CodeRange codeRange in codeRanges)
            foreach (CatalogEntry child in catalog.Get(codeRange.fromCode.parent).children)
            {
                CatalogCode temp = child.codePrefix;
                if (child.codePrefix.CompareTo(codeRange.fromCode) >= 0 &&
                    child.codePrefix.CompareTo(codeRange.toCode) <= 0)
                {
                    IsFile(temp, out string p, out bool isImage);
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
            List<CatalogCode> output = new List<CatalogCode>();
            foreach (CatalogEntry child in temp.children)
            {
                IsFile(child.codePrefix, out string p, out bool isImage);
                if (isImage)
                    output.Add(child.codePrefix);
                output.AddRange(CardsOf(child));
            }

            return output;
        }

        private void GenerateCardView(List<CatalogCode> codes)
        {
            ImageFactory output = new ImageFactory().Load(new Bitmap(1, 1));
            foreach (CatalogCode code in codes)
            {
                string path = CardPath(code);
                ImageLayer temp = new ImageLayer();
                temp.Image = Image.FromFile(path);
                temp.Position = new Point(0, output.Image.Height);
                ResizeLayer rl = new ResizeLayer(new Size(Math.Max(temp.Image.Width, output.Image.Width),
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
            PC.WriteLine("Insert the code you want to display (. for all):");
            string response = ReadAnswer();
            CatalogCode code = new CatalogCode(response);

            PC.WriteLine("And to what depth (-1 for all)?");
            if (!int.TryParse(ReadAnswer(), out int depth)) depth = -1;
            if (code.Equals(CatalogCode.current) && depth >= 1)
                depth++;
            Console.WriteLine(TreePrint(catalog.Get(code), depth));
        }

        private bool IsFile(CatalogCode code, out string path, out bool b)
        {
            path = null;
            b = false;
            string testFolder = folder + storage + FolderFor(code);
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
            return IsFile(code, out path, out bool b);
        }

        private bool IsFile(CatalogCode code)
        {
            return IsFile(code, out string p);
        }

        private string CardPath(CatalogCode code)
        {
            string testFolder = folder + storage + FolderFor(code);
            return Directory.EnumerateFiles(testFolder)
                .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).Contains(code.ToString()));
        }

        private void PromptDeleteFolder()
        {
            PC.FormatWriteLine("Insert the code to {-3}", "delete");
            CatalogCode code = new CatalogCode(ReadAnswer());
            YNAnswer response =
                AskYNQuestion($"Are you sure you want to delete the folder {catalog.Get(code).FancifyEntry()}?");
            if (response == YNAnswer.Yes)
            {
                catalog.Delete(code);
                DeleteFolderOfCode(code);
                Save(folder + fileLoc);
            }
        }

        private void DeleteFolderOfCode(CatalogCode code)
        {
            Directory.Delete(folder + storage + FolderFor(code), true);
        }

        private void PromptAddUpdateRecord(CatalogCode from = null)
        {
            CatalogCode code = from ?? CatalogCode.current + PromptCodeOrNewChild("Insert the new code to add");
            string title = PromptNewOrOldTitleToEdit(code);
            if (!catalog.Contains(code)) CreateFolderFor(code);
            catalog.Update(code, title);
            Save(folder + fileLoc);
        }

        private string PromptNewOrOldTitleToEdit(CatalogCode code)
        {
            YNAnswer response = YNAnswer.No;
            string title = "";
            if (catalog.Contains(code))
            {
                title = catalog.Get(code).name;
                if(title!="")
                    response = AskYNQuestion($"Would you like to keep the title {PC.Format("{0}",title)}? (Y/N)");
            }

            if (response == YNAnswer.No)
            {
                PC.FormatWriteLine("Insert the title of {0}",code);
                title = ReadAnswer();
            }

            return title;
        }

        private YNAnswer AskYNQuestion(string formattedString)
        {
            Console.WriteLine(formattedString);
            string response = ReadAnswer();
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

        private void PromptUpdateCatalogFromInput()
        {
            string[] pictures = Directory.GetFiles(folder + inputFolder);
            if (pictures.Length != 0)
            {
                foreach (string pic in Directory.GetFiles(folder + inputFolder))
                {
                    Console.Clear();
                    Console.WriteLine(TreePrint(catalog.root));
                    Process p = PromptOpening(pic);
                    CatalogCode code = PromptCodeOrNewChild("Set the code for this file");
                    string title = PromptNewOrOldTitleToEdit(code);

                    CreateFolderFor(code);
                    SetFileCode(pic, code,title);
                    catalog.Update(code, title);
                    p?.Kill();
                    Save(folder + fileLoc);
                }
            }
            else
            {
                PC.WriteLine("No files to upload\n");
            }
        }

        private CatalogCode PromptCodeOrNewChild(string promptMessage)
        {
            PC.WriteLine(promptMessage);
            string input = ReadAnswer();
            if (input == "")
                throw new CatalogError("Cannot alter the Root");

            if (input.EndsWith("."))
            {
                CatalogCode code = new CatalogCode(input.RemoveLast(1));
                return catalog.NewChild(code);
            }

            return new CatalogCode(input);
        }

        private void SetFileCode(string originalFile, CatalogCode code, string title)
        {
            string name = $"{code.ToString()} {title}";
            string path = folder + storage + FolderFor(code) + "\\" + name + Path.GetExtension(originalFile);
            string temppath = Directory.EnumerateFiles(Path.GetDirectoryName(path))
                .FirstOrDefault(o => Path.GetFileNameWithoutExtension(o) == name);
            if (temppath != null)
                File.Delete(temppath);
            File.Move(originalFile, path);
        }

        private Process PromptOpening(string pic)
        {
            Process p = null;
            YNAnswer response =
                AskYNQuestion($"Would you like to see {PC.Format("{0}",Path.GetFileName(pic))}? (Y/N)");
            if (response == YNAnswer.Yes)
                p = OpenFileProcess(pic);
            return p;
        }

        private static Process OpenFileProcess(string pic)
        {
            Process thisP = Process.GetCurrentProcess();
            Process p = Process.Start(pic);
            IntPtr s = thisP.MainWindowHandle;
            SetForegroundWindow(s);
            return p;
        }

        public static CatalogManager Setup(string path)
        {
            Directory.CreateDirectory(path + inputFolder);
            Directory.CreateDirectory(path + outputFolder);
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
        private void Save(string path,string contents)
        {
            File.Delete(path);
            File.Create(path).Close();
            File.AppendAllText(path, contents);
        }

        private string ReadAnswer()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            string output = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            return output;
        }
    }
}