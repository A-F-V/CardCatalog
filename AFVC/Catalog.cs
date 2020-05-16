using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void Add(CatalogCode code, string s)
        {
            CatalogCode parent = code.parent;
            if(parent.Depth==0)
                root.Add(code,s);
            if (Contains(parent))
            {
                Get(parent).Add(code, s);
            }
            else
            {
                Add(parent,"");
            }
        }

        private CatalogEntry Get(CatalogCode code)
        {
            return root.Get(code);
        }

        private bool Contains(CatalogCode codePrefix)
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
            public string name { get; private set; }
            private List<CatalogEntry> children = new List<CatalogEntry>();

            public void Add(CatalogCode code, string s)
            {
                children.Add(new CatalogEntry(code, s));
            }

            public CatalogEntry()
            {
                codePrefix = null;
            }

            public CatalogEntry(CatalogCode code, string s)
            {
                codePrefix = code;
                name = s;
            }

            public CatalogEntry Get(CatalogCode code)
            {
                if (code.Equals(CatalogCode.current))
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
        }
    }
}