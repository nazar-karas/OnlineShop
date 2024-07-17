using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class FeatureGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<FeatureDto> Features { get; set; }
    }
}
