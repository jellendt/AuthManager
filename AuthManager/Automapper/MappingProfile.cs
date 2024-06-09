using AuthManager.Entities;
using AuthManager.Models.Responses;
using AutoMapper;

namespace AuthManager.Automapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            _ = this.CreateMap<User, AuthenticateResponse>();
        }
    }
}
