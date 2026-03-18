using AutoMapper;
using ECommerce_Project.Api.DTOs.User;
using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Mapping

{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserEntity, UserResponseDto>();

            CreateMap<CreateUserDto, UserEntity>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            CreateMap<UpdateUserDto, UserEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsAdmin, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());
        }
    }
}
