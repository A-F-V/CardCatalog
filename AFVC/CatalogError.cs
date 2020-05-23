using System;

namespace AFVC
{
    internal class CatalogError : Exception
    {
        public CatalogError()
        {
        }

        public CatalogError(string message) : base(message)
        {
        }
    }
}