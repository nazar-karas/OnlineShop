using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Configuration
{
    public class RepeatedJobConfiguration
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string ClassName { get; set; }
        public string Interval { get; set; }
    }
}
