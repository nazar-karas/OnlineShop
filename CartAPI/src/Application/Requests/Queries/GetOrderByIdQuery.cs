using Domain.Documents;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests.Queries
{
    public class GetOrderByIdQuery : IRequest<Order>
    {
        public string Id { get; set; }
    }
}
