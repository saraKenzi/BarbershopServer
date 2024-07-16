using AutoMapper;
using BarbershopDL.EF.Models;
using BarbershopEntities.DTO.AppintmentDTO;
using BarbershopEntities.DTO.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopDL
{
    public class MapperManager:Profile
    {
        public MapperManager()
        {
            CreateMap<AppointmentToAddDTO, Appointment>();
            CreateMap<UserLoginDTO, User>();
            CreateMap<UserToAddDTO, User>();
            CreateMap< User, UserToResponseDTO>();


        }
    }
}
