using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MessageBroker
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private ILogger<OrderCreatedConsumer> _logger;
        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<OrderCreated> context)
        {
            string log = "Order with product " + context.Message.Name + " was stored";
            Console.WriteLine(log);
            _logger.LogInformation(log);
            return Task.CompletedTask;
        }
    }
}
