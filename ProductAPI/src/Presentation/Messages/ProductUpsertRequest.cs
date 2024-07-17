using Application.DTO;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Messages
{
    public class ProductUpsertRequest
    {
        public ProductDto Product { get; set; }
        public IEnumerable<AttachFileRequest> FilesToAttach { get; set; }
    }
}
