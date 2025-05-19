using AutoMapper;

namespace ProductsApi.Mappings
{
    public class ProductProfileMapping : Profile
    {
        public ProductProfileMapping()
        {
            CreateMap<Data.Entities.Product, Models.ProductModel>();
            CreateMap<Models.ProductModel, Data.Entities.Product>();

            CreateMap<Data.Entities.Product, Models.ProductTrimmedModel>();
        }
    }
}
