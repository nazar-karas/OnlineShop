using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Query
{
    public class ProductParams : Params
    {
        #region Filters

        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public bool? InStock { get; set; }

        #endregion
    }
}
