using Application.Common;
using Application.Requests.Queries;
using Azure.Core;
using Domain.Constants;
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
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<Order>>
    {
        private OrderRepository OrderRepository;

        public GetOrdersQueryHandler(OrderRepository orderRepository)
        {
            OrderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var fields = new List<FilterField>()
            {
                new FilterField()
                {
                    Name = nameof(Order.PhoneNumber),
                    Value = request.PhoneNumber
                },
                new FilterField()
                {
                    Name = nameof(Order.FirstName),
                    Value = request.FirstName
                },
                new FilterField()
                {
                    Name = nameof(Order.LastName),
                    Value = request.LastName
                }
            };

            var filters = fields.Where(x => !string.IsNullOrEmpty(x.Value)).ToArray();

            var direction = request.Order.Contains("asc", StringComparison.OrdinalIgnoreCase) ?
                SortDirection.Ascending :
                SortDirection.Descending;

            var sorting = new Sorting() { Field = request.SortBy, Direction = direction };

            var orders = await OrderRepository.GetAllAsync(filters, sorting);

            return orders;
        }
    }
}
