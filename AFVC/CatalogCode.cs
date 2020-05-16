using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    internal class CatalogCode : IComparable<CatalogCode>
    {
        private string original;
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

        public int CompareTo(CatalogCode other)
        {
            throw new NotImplementedException();
        }
    }
}