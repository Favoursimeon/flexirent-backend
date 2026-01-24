using AutoMapper;
using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;

namespace FlexiRent.Application.Mappings;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequest, User>();
        CreateMap<User, AuthResponse>();
    }
}