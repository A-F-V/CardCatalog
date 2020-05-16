using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            switch (dec)
            {
                case 0:
                    break;
                case 1:
                    break;
                default:
                    break;
            }
        }
    }
}
