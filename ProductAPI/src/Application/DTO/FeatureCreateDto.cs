using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class FeatureCreateDto
    {
        public string Name { get; set; }
        public List<string> PossibleValues { get; set; }
    }
}
