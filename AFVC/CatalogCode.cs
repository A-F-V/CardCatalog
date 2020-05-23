using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pastel;
using System.Drawing;
namespace AFVC
{
    internal class CatalogCode : IComparable<CatalogCode>
    {
        public static readonly CatalogCode current = new CatalogCode("");

        private string original;
        internal CatalogCode parent => new CatalogCode(Depth==1?"":original.Remove(original.Length-1-CodePattern[Depth-1].ToString().Length));

        public int[] CodePattern { get; private set; }
        public int Depth => CodePattern.Length;

        public CatalogCode(string codeOriginal)
        {
            original = codeOriginal.RemoveNonNumeric();
            generateCodePattern(original);
        }

        private void generateCodePattern(string codeOriginal)
        {
            try
            {
                if (codeOriginal == "" || codeOriginal == "root")
                    CodePattern = new int[0];
                else
                {
                    if(codeOriginal.Contains("-"))
                        throw new CatalogError();
                    CodePattern = codeOriginal.Split('.').Select(Int32.Parse).ToArray();
                }
            }
            catch (Exception e)
            {
                throw new CatalogError($"{codeOriginal} is not a valid code");
            }
        }

        public static CatalogCode Relative(CatalogCode elder, CatalogCode younger)
        {
            return new CatalogCode(younger.original.Remove(0,elder.original.Length+(elder.Depth==0?0:1)));
        }
        public int CompareTo(CatalogCode other)
        {
            if (Equals(other))
                return 0;
            else
            {
                if (Depth == 0) return -1;
                if (other.Depth == 0) return 1;
                for (int pos = 0; pos < Math.Min(Depth, other.Depth); pos++)
                {
                    int dif = CodePattern[pos].CompareTo(other.CodePattern[pos]);
                    if (dif != 0)
                        return dif;
                }

                return Depth.CompareTo(other.Depth);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CatalogCode temp)
            {
                if (!temp.Depth.Equals(Depth))
                    return false;
                for(int pos = 0; pos < Depth; pos++)
                    if (!temp.CodePattern[pos].Equals(CodePattern[pos]))
                        return false;
                return true;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return original;
        }

        public static bool SameFolder(CatalogCode codeA, CatalogCode codeB)
        {
            return codeA.parent.Equals(codeB.parent);
        }

        public int Youngest()
        {
            if (this == current)
                return 0;
            return CodePattern[Depth - 1];
        }

        public static CatalogCode operator +(CatalogCode a,CatalogCode b) => new CatalogCode(a.ToString()+ (a.Equals(current) ? "" : ".") + b.ToString());

        public static CatalogCode operator +(CatalogCode a, int diff)=> new CatalogCode(a.parent.ToString()+(a.parent.Equals(current)?"":".")+(a.Youngest()+diff).ToString());

        public CatalogCode Increment()
        {
            return this + 1;
        }
    }

}