using System;

namespace AFVC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CatalogManager m;
            if (args.Length != 0)
            {
                m = new CatalogManager(Loader.loaderFromFolder(args[0]), args[0]);
                m.Run();
            }
            else
            {
                Console.WriteLine("Files should be named correctly in input folder");
                Console.WriteLine(
                    "Few warnings are given in program. Ensure all codes are correct (01 is incorrect, 1 is correct)");
                string s;
                int dec;
                do
                {
                    Console.WriteLine("0 - New Card System\n1 - Load Card System\n2 - Close");
                    s = Console.ReadLine();
                } while (!int.TryParse(s, out dec));

                if (dec == 0 || dec == 1)
                {
                    string fileContent = string.Empty;
                    string filePath = string.Empty;

                    string path = MorePaths.getFolderPath();
                    if (path == null)
                        return;

                    m = dec == 0 ? CatalogManager.Setup(path) : new CatalogManager(Loader.loaderFromFolder(path), path);

                    Console.Clear();
                    m.Run();
                }
            }
        }
    }
}