using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class AttachedFileUpsertDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Location { get; set; }
        public string Checksum { get; set; }
        public Guid EntityId { get; set; }
    }
}
