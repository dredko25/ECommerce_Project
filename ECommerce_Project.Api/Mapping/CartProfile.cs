using AutoMapper;
using ECommerce_Project.Api.DTOs.Cart;
using ECommerce_Project.Api.DTOs.CartItem;
using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Mapping
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItemEntity, CartItemResponseDto>()
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(
                    dest => dest.ProductImageUrl,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null))
                .ForMember(
                    dest => dest.UnitPrice,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0m));

            CreateMap<CartEntity, CartResponseDto>()
                .ForMember(
                    dest => dest.Items,
                    opt => opt.MapFrom(src => src.CartItems));
        }
    }
}
