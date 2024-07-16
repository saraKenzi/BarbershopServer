using BarbershopDL.EF.Models;
using BarbershopEntities;
using BarbershopEntities.DTO.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopDL.Interfaces
{
    public interface IUserDL
    {
        Task<OperationResult<UserToResponseDTO>> AddUser(UserToAddDTO newUser);
        Task<OperationResult<UserToResponseDTO>> Login(UserLoginDTO user);
        Task<OperationResult<UserToResponseDTO>> GetUserDetailsByUserId(int userId);
        Task<OperationResult<List<UserToResponseDTO>>> GetUserIdByFirstName(string userName);

    }
}