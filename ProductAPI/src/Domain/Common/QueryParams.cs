using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class QueryParams
    {
        public int Page { get; set; }
        public int Number { get; set; }
        public Filter FilterBy { get; set; }
        public string SortBy { get; set; }
    }
}
