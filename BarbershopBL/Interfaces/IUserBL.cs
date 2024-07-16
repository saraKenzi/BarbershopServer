using BarbershopDL.EF.Models;
using BarbershopEntities;
using BarbershopEntities.DTO.UserDTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopBL.Interfaces
{
    public interface IUserBL
    {
       Task< OperationResult<UserToResponseDTO>> AddUser(UserToAddDTO newUser);
        Task<OperationResult<UserToResponseDTO>> Login(UserLoginDTO user);
        bool Logout();
        Task<OperationResult<UserToResponseDTO>> GetUserDetailsByUserId(string userId);
        Task<OperationResult<List<UserToResponseDTO>>> GetUserIdByFirstName(string userName);


    }
}
