using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class FeatureValueReadDto
    {
        public Guid ValueId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
