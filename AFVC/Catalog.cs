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

        public bool Contains(CodeRange range)
        {
            if (range == null)
                return false;
            else
            {
                CatalogCode counter = range.fromCode;
                while (counter.Youngest() <= range.toCode.Youngest())
                {
                    if (Get(counter) != null)
                        return true;
                    counter = counter.Increment();
                }

                return false;
            }
        }
        public bool Contains(CodeRange range,CodeRange usingCodeRange, int offset)
        {
            if (range == null)
                return false;
            else
            {
                CatalogCode counter1 = range.fromCode;
                CatalogCode counter2 = usingCodeRange.fromCode+offset;
                while (counter1.Youngest() <= range.toCode.Youngest())
                {
                    if (Contains(counter2)&&Contains(counter1))
                        return true;
                    counter1 = counter1.Increment();
                    counter2 = counter2.Increment();
                }

                return false;
            }
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

        public CatalogCode NewChild(CatalogCode code)
        {
            var ce = Get(code);
            if(ce==null||ce.children.Count==0)
                return new CatalogCode(code.ToString()+(code.Equals(CatalogCode.current)?"":".")+"0");
            else
            {
                int max = ce.children.Max(c => c.codePrefix.Youngest());
                int addition=0;
                if (max+1 == ce.children.Count)
                    addition = max + 1;
                else
                {
                    List<CatalogEntry> children = ce.children;
                    if (children[0].codePrefix.Youngest() != 0)
                        addition = 0;
                    else
                    {
                        CatalogCode prev = children[0].codePrefix;
                        int pos = 1;
                        while (pos < children.Count)
                        {
                            CatalogCode comp = children[pos].codePrefix;
                            if (comp.Youngest() - prev.Youngest() != 1)
                            {
                                addition = prev.Youngest() + 1;
                                break;
                            }
                            prev = comp;
                            pos++;
                        }
                    }
                }
                if(code.Equals(CatalogCode.current))
                    return new CatalogCode(addition.ToString());
                return new CatalogCode(code.ToString()+$".{addition}");
            }
        }
    }
}