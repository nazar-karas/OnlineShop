using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Feature
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? PossibleValues { get; set; }

        #region Navigation Properties
        public Guid FeatureGroupId { get; set; }
        public FeatureGroup FeatureGroup { get; set; }
        public List<FeatureValue> FeatureValues { get; set; }
        #endregion
    }
}
