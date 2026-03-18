using AutoMapper;
using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductEntity, ProductResponseDto>()
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<ProductEntity, ProductSummaryDto>()
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<CreateProductDto, ProductEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            CreateMap<UpdateProductDto, ProductEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());
        }
    }
}
