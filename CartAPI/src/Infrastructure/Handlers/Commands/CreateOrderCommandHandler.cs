using Application.Requests.Commands;
using Domain.Documents;
using Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Handlers.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
    {
        private OrderRepository OrderRepository;
        private ProductRepository ProductRepository;
        public CreateOrderCommandHandler(OrderRepository orderRepository, ProductRepository productRepository)
        {
            OrderRepository = orderRepository;
            ProductRepository = productRepository;
        }
        public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order();

            order.Id = Guid.NewGuid().ToString();
            order.FirstName = request.FirstName;
            order.LastName = request.LastName;
            order.PhoneNumber = request.PhoneNumber;
            
            order.Payment = new Payment()
            {
                Id = Guid.NewGuid().ToString(),
                Type = request.PaymentType,
                Status = request.PaymentStatus
            };

            order.CarrierId = request.CarrierId;
            order.IsShipped = request.IsShipped;
            order.Products = new List<Product>();

            var availableProducts = await ProductRepository.GetAllAsync();

            foreach (var product in request.Products)
            {
                var rightProduct = availableProducts.SingleOrDefault(x => x.Id == product.Id);

                if (rightProduct != null)
                {
                    order.Products.Add(rightProduct);
                }
            }

            await OrderRepository.CreateAsync(order);

            return order;
        }
    }
}
