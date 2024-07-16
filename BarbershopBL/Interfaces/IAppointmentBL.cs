using BarbershopDL.EF.Models;
using BarbershopEntities;
using BarbershopEntities.DTO.AppintmentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopBL.Interfaces
{
    public interface IAppointmentBL
    {
       Task <OperationResult<List<Appointment>>> GetAllAppointments(int page, int perPage);
       Task<OperationResult< Appointment >> CreateNewAppointment(AppointmentToAddDTO appointment);
       Task<OperationResult<Appointment>> UpdateAppointmentDate(string appointmentId,AppointmentToAddDTO newAppointmentDate);

        Task<OperationResult<Appointment>> DeleteAppointment(string appointmentId);
        Task<OperationResult<int>> GetFutureAppointmentsCount();



    }
}
