using System;
using System.Collections.Generic;
using System.Drawing;
using Pastel;

namespace AFVC
{
    internal class CatalogEntry : IComparable<CatalogEntry>
    {
        public List<CatalogEntry> children = new List<CatalogEntry>();
        public CatalogCode codePrefix { get; }
        public string name { get; set; } = "";
        public string FileName => $"{codePrefix.ToString()} {name.ToString()}";

        public CatalogEntry()
        {
            codePrefix = CatalogCode.current;
        }

        public CatalogEntry(CatalogCode code, string s)
        {
            codePrefix = code;
            name = s;
        }

        public int CompareTo(CatalogEntry other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return codePrefix.CompareTo(other.codePrefix);
        }

        public void Add(CatalogCode code, string s)
        {
            children.Add(new CatalogEntry(code, s));
            children.Sort();
        }

        public CatalogEntry Get(CatalogCode code)
        {
            if (codePrefix.Equals(code)) return this;

            CatalogCode rel = CatalogCode.Relative(codePrefix, code);
            int ID = rel.CodePattern[0];
            CatalogEntry child = GetChild(ID);
            return child != null ? child.Get(code) : null;
        }

        public CatalogEntry GetChild(int ID)
        {
            foreach (CatalogEntry ce in children)
            {
                CatalogCode c = ce.codePrefix;
                if (c.CodePattern[c.Depth - 1] == ID) return ce;
            }

            return null;
        }

        public IEnumerable<string> Serialize()
        {
            List<string> output = new List<string>();
            output.Add($"{codePrefix},{name}");
            foreach (CatalogEntry child in children) output.AddRange(child.Serialize());

            return output;
        }

        public List<CatalogEntry> Search(string query)
        {
            List<CatalogEntry> output = new List<CatalogEntry>();
            if (!codePrefix.Equals(CatalogCode.current) && name.ToLower().Contains(query))
                output.Add(this);
            foreach (CatalogEntry child in children) output.AddRange(child.Search(query));

            return output;
        }

        public string FancifyEntry(bool over = false, int number=0)
        {
            PastelConsole PC = new PastelConsole(ColourPalette.MarineFields);
            return PC.Format($"{{-2}} {{{(over?number:Math.Max(codePrefix.Depth - 1,0))}}}", codePrefix,name ?? String.Empty);
        }
    }
}