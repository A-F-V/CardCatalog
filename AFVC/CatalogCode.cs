using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    internal class CatalogCode : IComparable<CatalogCode>
    {
        public static readonly CatalogCode current = new CatalogCode("");

        private string original;
        internal CatalogCode parent => new CatalogCode(original.Remove(original.Length-1-CodePattern[Depth-1].ToString().Length));

        public int[] CodePattern { get; private set; }
        public int Depth => CodePattern.Length;

        public CatalogCode(string codeOriginal)
        {
            original = codeOriginal;
            generateCodePattern(codeOriginal);
        }

        private void generateCodePattern(string codeOriginal)
        {
            CodePattern = codeOriginal.Split('.').Select(Int32.Parse).ToArray();
        }

        public static CatalogCode Relative(CatalogCode elder, CatalogCode younger)
        {
            return new CatalogCode(younger.original.Remove(0,elder.original.Length));
        }
        public int CompareTo(CatalogCode other)
        {
            throw new NotImplementedException();
        }
    }

}