using Application.Requests.Queries;
using Domain.Documents;
using Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Handlers.Queries
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Order>
    {
        private OrderRepository OrderRepository;

        public GetOrderByIdQueryHandler(OrderRepository orderRepository)
        {
            OrderRepository = orderRepository;
        }

        public async Task<Order> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await OrderRepository.GetAsync(request.Id);

            return order;
        }
    }
}
