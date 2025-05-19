using AutoMapper;
using ProductsApi.Data.Entities;
using ProductsApi.Models;
namespace ProductsApi.Infrastructure.Mappings
{
    public class ProductProfileMapping : Profile
    {
        public ProductProfileMapping()
        {
            CreateMap<Data.Entities.Product, Models.ProductModel>();
            CreateMap<Models.ProductModel, Data.Entities.Product>();
            CreateMap<Product, ProductTrimmedModel>();
        }
    }
}
