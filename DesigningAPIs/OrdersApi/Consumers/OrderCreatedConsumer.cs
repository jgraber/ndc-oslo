using MassTransit;
using OrdersApi.Controllers;

namespace OrdersApi.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            await Task.Delay(1000);
            Console.WriteLine($"{context.Message.OrderId} - {context.Message.CreatedAt}");
        }
    }
}
