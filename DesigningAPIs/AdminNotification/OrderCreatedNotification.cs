using Contracts.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminNotification
{
    public class OrderCreatedNotification : IConsumer<OrderCreated>
    {
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            await Task.Delay(1000);
            Console.WriteLine($"I was just notified of a new order with OrderId:{context.Message.OrderId}, that was created At:{context.Message.CreatedAt}");

        }
    }
}
