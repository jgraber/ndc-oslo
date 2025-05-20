using Grpc.Core;
using Stocks.Repository;

namespace Stocks.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly IProductStockRepository _productStockRepository;

        public GreeterService(ILogger<GreeterService> logger, IProductStockRepository productStockRepository)
        {
            _logger = logger;
            _productStockRepository = productStockRepository;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<ProductStockList> GetStock(StockRequest request, ServerCallContext context)
        {
            var productIds = request.ProductId.ToList();
            var stocks = _productStockRepository.GetProductStocksAsync(productIds).Result;

            ProductStockList productStockList = new ProductStockList();

            foreach (var item in stocks)
            {
                productStockList.Products.Add(new ProductStock
                {
                    ProductId = item.ProductId,
                    Stock = item.Stock
                });

            }

            return Task.FromResult(productStockList);
        }
    }
}
