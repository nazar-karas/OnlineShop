using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class ReviewedContent
    {
        public string ClientId { get; set; }
        public List<Guid> ReviewedProducts { get; set; }
    }
}
