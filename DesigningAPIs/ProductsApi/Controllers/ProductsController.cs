using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProductsApi.Data.Entities;
using ProductsApi.Mappings;
using ProductsApi.Models;
using ProductsApi.Service;

namespace ProductsApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache memoryCache;

        private readonly ILogger<ProductsController> logger;
        private const string LimitedStockProductsKey = "LSPC";

        public ProductsController(IProductService productService, IMapper mapper, IMemoryCache memoryCache, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _mapper = mapper;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetProductsAsync();

            if (HttpContext.Request.Headers["Consumer"] == "McDonalds")
            {
                return Ok(_mapper.Map<List<ProductTrimmedModel>>(products));
            }

            var models = _mapper.Map<List<ProductModel>>(products);
            return Ok(models);
        }


        // GET: api/Products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<ProductModel>(product);
            return Ok(model);
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            try
            {
                await _productService.UpdateProductAsync(product);
            }
            catch
            {
                if (!await _productService.ProductExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var createdProduct = await _productService.AddProductAsync(product);
            var model = _mapper.Map<ProductModel>(createdProduct);
            return CreatedAtAction("GetProduct", new { id = createdProduct.Id }, model);
        }


        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.DeleteProductAsync(id);
            return NoContent();
        }


        // GET: api/Products/{id}
        [HttpHead("{id}")]
        public async Task<IActionResult> CheckIfProductExists(int id)
        {
            var product = await _productService.ProductExistsAsync(id);
            if (product == false)
            {
                return NotFound();
            }

            return Ok();
        }


        // GET: api/products/limitedstock
        [HttpGet]
        [Route("limitedstock")]
        [Produces(typeof(Product[]))]
        public async Task<IEnumerable<Product>> GetLimitedStockProducts()
        {
            // Try to get the cached value.
            if (!memoryCache.TryGetValue(LimitedStockProductsKey, out Product[]? cachedValue))
            {
                logger.LogInformation("go to the database");

                // If the cached value is not found, get the value from the database.
                var products = await _productService.GetProductsAsync();
                cachedValue = products.Where(p => p.Stock <= 30)
                    .ToArray();


                MemoryCacheEntryOptions cacheEntryOptions = new()
                { //AbsoluteExpiration = DateTimeOffset.UtcNow,
                    SlidingExpiration = TimeSpan.FromSeconds(30),
                    Size = cachedValue?.Length
                };

                memoryCache.Set(LimitedStockProductsKey, cachedValue, cacheEntryOptions);
            }
            MemoryCacheStatistics? stats = memoryCache.GetCurrentStatistics();
            logger.LogInformation($"Memory cache. Total hits: {stats?
                .TotalHits}. Estimated size: {stats?.CurrentEstimatedSize}.");
            return cachedValue ?? Enumerable.Empty<Product>();
        }

    }

}
