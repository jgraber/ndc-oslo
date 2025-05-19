namespace ProductsApi.Models
{
    public class ProductTrimmedModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// The description of the product.
        /// </summary>
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal FinalPrice
        {
            get
            {
                return this.Price * (decimal)1.19;
            }
        }
    }

}
