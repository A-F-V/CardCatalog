using System;
using System.Collections.Generic;
using System.Drawing;
using Pastel;

namespace AFVC
{
    partial class Catalog
    {
        private class CatalogEntry : IComparable<CatalogEntry>
        {
            public CatalogCode codePrefix { get; private set; }
            public string name { get; set; }
            public List<CatalogEntry> children = new List<CatalogEntry>();
            public Color[] colors = new Color[] {
                Color.Crimson,
                Color.FromArgb(213,254,119),
                Color.FromArgb(57,240,119),
                Color.FromArgb(0,201,167),
                Color.MediumPurple,
                Color.DarkMagenta,
                Color.Fuchsia,
                Color.Gold);
            };
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

            public override string ToString()
            {
                string thisString = $"|-{codePrefix.ToString().Pastel(Color.OrangeRed)}. {(name==null?string.Empty:name.Pastel(colors[Math.Min(codePrefix.Depth-1,colors.Length-1)]))}\n";
                foreach (var child in children)
                {
                    thisString += new String(' ', (codePrefix.Depth+1)*3) + child.ToString();
                }

                return thisString;
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
        }
    }
}