using Microsoft.EntityFrameworkCore;

namespace Stocks.Repository
{

    [Keyless]
    public class ProductStock
    {
        public int ProductId { get; set; }
        public int Stock { get; set; }
    }
}