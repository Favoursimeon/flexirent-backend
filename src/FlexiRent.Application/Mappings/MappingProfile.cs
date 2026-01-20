using AutoMapper;
using FlexiRent.Domain.Entities;

namespace FlexiRent.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, Profile>().ReverseMap();
            // Add DTO mappings here
        }
    }
}
