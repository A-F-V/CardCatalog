using System.Drawing;
using System.Linq;
using Pastel;

namespace AFVC
{
    internal static class StringExtensions
    {
        private static readonly char[] numeric =
        {
            '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static string RemoveLast(this string s, int characters)
        {
            return s.Remove(s.Length - characters);
        }

        public static string RemoveFirst(this string s, int characters)
        {
            return s.Remove(0, characters);
        }

        public static string RemoveNonNumeric(this string s)
        {
            return string.Concat(s.Where(c => numeric.Contains(c)));
        }

        public static string Tone(this string s, Color c)
        {
            return s.Pastel(c);
        }
    }
}