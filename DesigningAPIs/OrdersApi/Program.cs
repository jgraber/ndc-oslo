
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using OrdersApi.Data;
using OrdersApi.Data.Repositories;
using OrdersApi.Infrastructure;
using OrdersApi.Service;
using OrdersApi.Service.Clients;
using OrdersApi.Services;
using Polly;
using Polly.Hedging;
using System.Net;

namespace OrdersApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            builder.Services.AddAutoMapper(typeof(OrderProfileMapping).Assembly);
            builder.Services.AddDbContext<OrderContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
           
            builder.Services.AddHttpClient<IProductStockServiceClient, ProductStockServiceClient>()
                .AddResilienceHandler("retry-policy", options =>
                {
                    // Configure standard resilience options here
                    //options.AddRetry(new HttpRetryStrategyOptions
                    //{
                    //    MaxRetryAttempts = 4,
                    //    Delay = TimeSpan.FromSeconds(2),
                    //    BackoffType = DelayBackoffType.Exponentia

                    //});

                    //options.AddHedging(new HedgingStrategyOptions<HttpResponseMessage>()
                    //{
                    //    MaxHedgedAttempts = 3,
                    //    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    //    .Handle<ArgumentOutOfRangeException>()
                    //    .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
                    //                        Delay = TimeSpan.FromSeconds(1),
                    //});

                   // options.AddTimeout(TimeSpan.FromSeconds(5));


                });




            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
