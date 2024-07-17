using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AttachedFile
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? Location { get; set; }
        public string? Checksum { get; set; }

        #region Navigation Properties
        public Guid? ProductId { get; set; }
        public Product Product { get; set; }
        #endregion
    }
}
