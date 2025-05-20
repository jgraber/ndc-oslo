using Stocks.Repository;

public interface IProductStockRepository
{
    Task<List<ProductStock>> GetProductStocksAsync(List<int> productIds);
}