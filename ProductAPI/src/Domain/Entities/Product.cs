using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }

        #region Navigation Properties
        public Guid? FeatureGroupId { get; set; }
        public FeatureGroup FeatureGroup { get; set; }
        public IEnumerable<FeatureValue> FeaturesValues { get; set; }
        public IEnumerable<AttachedFile> AttachedFiles { get; set; }
        #endregion
    }
}
