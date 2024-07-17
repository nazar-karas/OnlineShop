using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FeatureValue
    {
        public Guid Id { get; set; }
        public string Value { get; set; }

        #region Navigation Properties
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; }
        #endregion
    }
}
