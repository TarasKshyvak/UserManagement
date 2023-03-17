using AutoMapper;
using UM.BLL.Models;
using UM.DAL.Entities;

namespace UM.BLL
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<User, UserModel>().ReverseMap();
        }
    }
}
