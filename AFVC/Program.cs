using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AFVC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Files should be named correctly in input folder");
            Console.WriteLine("Few warnings are given in program. Ensure all codes are correct (01 is incorrect, 1 is correct)");
            string s;
            int dec;
            do
            {
                Console.WriteLine("0 - New Card System\n1 - Load Card System\n2 - Close");
                s = Console.ReadLine();

            } while (!Int32.TryParse(s,out dec));

            if (dec == 0 || dec == 1)
            {
                var fileContent = string.Empty;
                var filePath = string.Empty;

                string path = getFolderPath();
                if(path==null)
                    return;
                CatalogManager m;
                if (dec == 0)
                {
                    m = CatalogManager.Setup(path);
                }
                else
                {
                    m = new CatalogManager(Loader.loaderFromFolder(path),path);
                }
                Console.Clear();
                m.Run();
            }
        }

        private static string getFolderPath()
        {
            string selectedPath=null;
            var t = new Thread((ThreadStart)(() => {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = System.Environment.SpecialFolder.MyComputer;
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == DialogResult.Cancel)
                    return;

                selectedPath = fbd.SelectedPath;
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return selectedPath;
        }
    }
}
