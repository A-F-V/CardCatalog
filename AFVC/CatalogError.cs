using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    class CatalogError : Exception
    {
        public CatalogError() : base()
        {
            
        }
        public CatalogError(String message) : base(message)
        {
            
        }
    }
}
