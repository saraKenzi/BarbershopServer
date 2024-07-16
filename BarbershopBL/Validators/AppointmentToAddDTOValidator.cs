using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarbershopEntities.DTO.AppintmentDTO;
using FluentValidation;
namespace BarbershopBL.Validators
{

    public class AppointmentToAddDTOValidator : AbstractValidator<AppointmentToAddDTO>
    {
        public AppointmentToAddDTOValidator()
        {
            RuleFor(x => x.AppointmentDate)
                .NotEmpty()
                .WithMessage("Appointment date is required.")
                .Must(BeAValidDate)
                .WithMessage("The date you selected has already passed.");
        }

        private bool BeAValidDate(DateTime date)
        {
            return date >= DateTime.Now;
        }

    }
}

