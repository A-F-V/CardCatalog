using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    static class StringExtensions
    {
        public static string RemoveLast(this string s, int characters)
        {
            return s.Remove(s.Length - characters);
        }

        public static string RemoveFirst(this string s, int characters)
        {
            return s.Remove(0, characters);
        }
    }
}
