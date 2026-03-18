using AutoMapper;
using ECommerce_Project.Api.DTOs.Order;
using ECommerce_Project.Api.DTOs.OrderItem;
using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderItemEntity, OrderItemResponseDto>()
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "This product does not exist."))
                .ForMember(
                    dest => dest.ProductImageUrl,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null));

            CreateMap<OrderEntity, OrderResponseDto>()
                .ForMember(
                    dest => dest.UserFullName,
                    opt => opt.MapFrom(src => src.User != null
                        ? $"{src.User.FirstName} {src.User.LastName}"
                        : string.Empty))
                .ForMember(
                    dest => dest.UserEmail,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<OrderEntity, OrderSummaryDto>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(
                    dest => dest.ItemCount,
                    opt => opt.MapFrom(src => src.OrderItems.Count));

            CreateMap<CreateOrderItemDto, OrderItemEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore());

            CreateMap<CreateOrderDto, OrderEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(
                    dest => dest.OrderItems,
                    opt => opt.MapFrom(src => src.Items));
        }
    }
}
