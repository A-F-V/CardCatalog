using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AFVC
{
    internal class CatalogEntry : IComparable<CatalogEntry>
    {
        public CatalogCode codePrefix { get; private set; }
        public string name { get; set; }
        public List<CatalogEntry> children = new List<CatalogEntry>();
        
        public void Add(CatalogCode code, string s)
        {
            children.Add(new CatalogEntry(code, s));
            children.Sort();
        }

        public CatalogEntry()
        {
            codePrefix = CatalogCode.current;
        }

        public CatalogEntry(CatalogCode code, string s)
        {
            codePrefix = code;
            name = s;
        }

        public CatalogEntry Get(CatalogCode code)
        {
            if (codePrefix.Equals(code))
            {
                return this;
            }
            else
            {
                CatalogCode rel = CatalogCode.Relative(codePrefix, code);
                int ID = rel.CodePattern[0];
                CatalogEntry child = GetChild(ID);
                return child != null ? child.Get(code) : null;
            }
        }

        public CatalogEntry GetChild(int ID)
        {
            foreach (var ce in children)
            {
                CatalogCode c = ce.codePrefix;
                if (c.CodePattern[c.Depth - 1] == ID)
                {
                    return ce;
                }
            }
            return null;
        }

        public IEnumerable<string> Serialize()
        {
            List<string> output = new List<string>();
            output.Add($"{codePrefix},{name}");
            foreach (var child in children)
            {
                output.AddRange(child.Serialize());
            }

            return output;
        }

        public int CompareTo(CatalogEntry other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return codePrefix.CompareTo(other.codePrefix);
        }

        public List<CatalogEntry> Search(string query)
        {
            List<CatalogEntry> output = new List<CatalogEntry>();
            if (!codePrefix.Equals(CatalogCode.current) && name.ToLower().Contains(query))
                output.Add(this);
            foreach (var child in children)
            {
                output.AddRange(child.Search(query));
            }

            return output;
        }

        public string FancifyEntry()
        {
            return $"{this.codePrefix.ToString().Pastel(Color.OrangeRed)}" +
                   $" {(this.name == null ? string.Empty : this.name.Pastel(CatalogManager.Colors[Math.Min(this.codePrefix.Depth - 1, CatalogManager.Colors.Length - 1)]))}";
        }
    }

}