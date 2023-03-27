using AutoMapper;
using UserManagement.Models;
using UserManagement.Entities;

namespace UserManagement
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<User, UserModel>().ReverseMap();
        }
    }
}
