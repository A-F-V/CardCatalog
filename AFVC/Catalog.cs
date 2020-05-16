using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pastel;

namespace AFVC
{
    class Catalog
    {
        private CatalogEntry root;

        public Catalog(Dictionary<string, string> dict)
        {
            root = new CatalogEntry();
            GenerateCatalog(dict);
        }

        private void GenerateCatalog(Dictionary<string, string> dict)
        {
            foreach (var key in dict.Keys)
            {
                CatalogCode code = new CatalogCode(key);
                Add(code, dict[key]);
            }
        }

        public void Update(CatalogCode code, String title)
        {
            if (Contains(code))
                Set(code, title);
            else
                Add(code, title);
        }

        public void Add(CatalogCode code, string s)
        {
            CatalogCode parent = code.parent;
            if (parent.Depth == 0)
            {
                root.Add(code, s);
                return;
            }
            if (!Contains(parent))
            {
                Add(parent, "");
            }
            Get(parent).Add(code, s);
        }

        private CatalogEntry Get(CatalogCode code)
        {
            return root.Get(code);
        }

        public void Delete(CatalogCode code)
        {
            if (Contains(code))
            {
                if (code.Depth == 1)
                    root.children.RemoveAll(e => e.codePrefix.Equals(code));
                else
                {
                    Get(code.parent).children.RemoveAll(e => e.codePrefix.Equals(code));
                }
            }
        }

        public bool Contains(CatalogCode codePrefix)
        {
            return Get(codePrefix) != null;
        }

        public Catalog()
        {
            root = new CatalogEntry();
        }

        private class CatalogEntry
        {
            public CatalogCode codePrefix { get; private set; }
            public string name { get; set; }
            public List<CatalogEntry> children = new List<CatalogEntry>();

            public void Add(CatalogCode code, string s)
            {
                children.Add(new CatalogEntry(code, s));
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
                string thisString = $"|-{codePrefix.ToString().Pastel(Color.OrangeRed)}. {(name==null?string.Empty:name.Pastel(Color.Crimson))}\n";
                foreach (var child in children)
                {
                    thisString += new String(' ', (codePrefix.Depth+1)*2) + child.ToString();
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
        }

        public override string ToString()
        {
            return root.ToString();
        }

        public void Set(CatalogCode code, string title)
        {
            Get(code).name = title;
        }

        public List<string> Serialize()
        {
            List<string> output = new List<string>();
            foreach (var child in root.children)
            {
                output.AddRange(child.Serialize());
            }

            return output;
        }
    }
}