using BarbershopEntities.DTO.UserDTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopBL.Validators
{
    public class UserLoginDTOValidator : AbstractValidator<UserLoginDTO>
    {
        public UserLoginDTOValidator()
        {
            RuleFor(user => user.UserName)
                .NotEmpty().WithMessage("User Name is required.")
                .Matches("^[a-zA-Z0-9]+$").WithMessage("User Name must contain only English letters and numbers.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Matches("^[0-9]{4,}$").WithMessage("Password must contain at least 4 digits.");
        }
    }
}
