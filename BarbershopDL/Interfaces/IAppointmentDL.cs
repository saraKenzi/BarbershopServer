using BarbershopDL.EF.Models;
using BarbershopEntities;
using BarbershopEntities.DTO.AppintmentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopDL.Interfaces
{
    public interface IAppointmentDL
    {
       Task<OperationResult< List<Appointment>>> GetAllAppointments(int page, int perPage );
        Task<OperationResult<Appointment>> CreateNewAppointment(AppointmentToAddDTO appointment,int userId);
        Task<OperationResult<Appointment>> UpdateAppointmentDate(int appointmentId,AppointmentToAddDTO newAppointmentDate, int userId);
        Task<OperationResult<Appointment>>  DeleteAppointment(int appointmentId,int userId);
        Task<OperationResult<int>> GetFutureAppointmentsCount();

    }
}
