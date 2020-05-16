using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AFVC
{
    class Program
    {
        static void Main(string[] args)
        {
            string s;
            int dec;
            do
            {
                Console.WriteLine("0 - New Card System\n1- Load Card System\n2 - Close");
                s = Console.ReadLine();

            } while (Int32.TryParse(s,out dec));

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
                    m = new CatalogManager(path);
                }
                else
                {
                    m = new CatalogManager(Loader.loaderFromFolder(path),path);
                }

                m.Run();
            }
        }

        private static string getFolderPath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }

                return null;
            }
        }
    }
}
