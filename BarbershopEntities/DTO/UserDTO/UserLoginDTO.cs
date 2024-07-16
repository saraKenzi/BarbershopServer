using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopEntities.DTO.UserDTO
{
    public class UserLoginDTO
    {
        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

    }
}
