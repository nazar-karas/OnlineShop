using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class ClientReview
    {
        public string Id { get; set; }
        public List<Guid> ProductsIds { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
