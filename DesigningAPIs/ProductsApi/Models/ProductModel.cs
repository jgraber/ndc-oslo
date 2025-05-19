namespace ProductsApi.Models
{
    public class ProductModel
    {
        /// <summary>
        /// Product Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the Product
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string LongDescription { get; set; }
        public int CategoryId { get; set; }
    }
}
