using AutoMapper;
using ECommerce_Project.Api.DTOs.Category;
using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Mapping
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CategoryEntity, CategoryResponseDto>()
                .ForMember(
                    dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products.Count));

            CreateMap<CreateCategoryDto, CategoryEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, CategoryEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        }
    }
}
