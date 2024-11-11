using Domain.Documents;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests.Commands
{
    public class CreateOrderCommand : IRequest<Order>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsShipped { get; set; }
        public List<ProductDto> Products { get; set; }
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; }
        public string CarrierId { get; set; }
    }

    public class ProductDto
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
    }
}
