using BarbershopEntities.DTO.UserDTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace BarbershopBL.Validators
{
    public class UserToAddDTOValidator : AbstractValidator<UserToAddDTO>
    {
        public UserToAddDTOValidator()
        {
            RuleFor(user => user.FirstName)
                .NotEmpty().WithMessage("First name is required.");
            RuleFor(user => user.LastName)
                .NotEmpty().WithMessage("Last name is required.");
            RuleFor(user => user.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^05\d{8}$").WithMessage("Phone number must be 10 digits starting with 05.");
            RuleFor(user => user.UserName)
                .NotEmpty().WithMessage("User Name is required.")
                .Matches("^[a-zA-Z0-9]+$").WithMessage("User Name must contain only English letters and numbers.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Matches("^[0-9]{4,}$").WithMessage("Password must contain at least 4 digits.");
        }
    }
}