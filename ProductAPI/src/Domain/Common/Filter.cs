using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class Filter
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Role { get; set; }
        public bool? IsInWhiteList { get; set; }
    }
}
