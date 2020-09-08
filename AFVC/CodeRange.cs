using System;
using System.Collections.Generic;

namespace AFVC
{
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
            string[] codes = range.Split('-');
            int l1 = codes[0].Length;
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
            List<CodeRange> output = new List<CodeRange>();
            foreach (string range in readAnswer.Split(',')) output.Add(new CodeRange(range));

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