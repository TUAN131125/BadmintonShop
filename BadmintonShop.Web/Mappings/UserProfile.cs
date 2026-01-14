using AutoMapper;
using BadmintonShop.Core.Entities;
using BadmintonShop.Web.Areas.Admin.ViewModels;

namespace BadmintonShop.Web.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // 1. Admin Map
            CreateMap<User, AdminUserVM>().ReverseMap()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Không map password trực tiếp vào hash

            // 2. Customer Map
            CreateMap<User, CustomerUserVM>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                // Khi map ngược từ Form về DB, gán Username = Email cho Customer
                .ReverseMap()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}