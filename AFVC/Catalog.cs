using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    partial class Catalog
    {
        public CatalogEntry root;
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

        public CatalogEntry Get(CatalogCode code)
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

        public List<CatalogEntry> Search(string query)
        {
            return root.Search(query.ToLower());
        }
    }
}