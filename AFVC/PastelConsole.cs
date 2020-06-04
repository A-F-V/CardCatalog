using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pastel;

namespace AFVC
{
    class PastelConsole
    {
        private ColourPalette palette;

        public PastelConsole(ColourPalette palette)
        {
            this.palette = palette;
        }

        public string Format(string literal, params object[] bindings)
        {
            string output = "";
            string[] temp = literal.Split('{', '}');
            for (int pos = 0; pos < temp.Length; pos++)
            {
                if (pos % 2 == 0)
                {
                    output += temp[pos].Pastel(palette.Body);
                }
                else
                {
                    output += bindings[(pos - 1) / 2].ToString().Pastel(palette[Int32.Parse(temp[pos])]);
                }
            }

            return output;
        }
        public void FormatWriteLine(string literal, params object[] bindings)
        {
            Console.WriteLine(Format(literal,bindings));
        }

        public void FormatWrite(string literal, params object[] bindings)
        {
            Console.Write(Format(literal,bindings));
        }
        public void WriteLine(string literal)
        {
            Console.WriteLine(literal.Pastel(palette.Body));
        }
    }
}
