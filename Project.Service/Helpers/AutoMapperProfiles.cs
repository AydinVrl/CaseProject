using AutoMapper;
using Project.Core.DTOs;
using Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Service.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
