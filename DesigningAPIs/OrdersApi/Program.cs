
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using OrdersApi.Consumers;
using OrdersApi.Data;
using OrdersApi.Data.Repositories;
using OrdersApi.Mappings;
using OrdersApi.Service;
using OrdersApi.Service.Clients;
using OrdersApi.Services;
using Polly;

namespace OrdersApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(OrderProfileMapping).Assembly);
            builder.Services.AddDbContext<OrderContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient<IProductStockServiceClient, ProductStockServiceClient>()
                .AddResilienceHandler("my-pipeline", builder =>
                {
                    // Refer to https://www.pollydocs.org/strategies/retry.html#defaults for retry defaults
                    builder.AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 4,
                        Delay = TimeSpan.FromSeconds(2),
                        BackoffType = DelayBackoffType.Exponential
                    });
                });
            builder.Services.AddGrpcClient<Stocks.Greeter.GreeterClient>(o =>
            {
                o.Address = new Uri("https://localhost:7277");
            });

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedConsumer>();
                // Step 2: Select a Transport
                x.UsingRabbitMq((context, cfg) => {
                    // Step 3: Configure the Transport

                    // Step 4: Configure Endpoints
                    // All consumers registered in step 1, will get
                    // default endpoints created.
                    cfg.ConfigureEndpoints(context);
                });
            });



            var app = builder.Build();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<OrderContext>().Database.EnsureCreated();
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
