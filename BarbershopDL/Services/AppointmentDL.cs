using BarbershopDL.Interfaces;
using BarbershopDL.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarbershopDL.EF.Contexts;
using Microsoft.EntityFrameworkCore;
using BarbershopEntities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using BarbershopEntities.DTO.AppintmentDTO;
using Microsoft.Extensions.Logging;
using BarbershopEntities.DTO.UserDTO;
using Microsoft.VisualBasic;

namespace BarbershopDL.Services
{
    public class AppointmentDL : IAppointmentDL
    {


        private readonly BarbershopContext _barbershopContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentDL> _logger;
        public AppointmentDL(BarbershopContext barbershopContext, IMapper mapper, ILogger<AppointmentDL> logger)
        {
            _barbershopContext = barbershopContext;
            _mapper = mapper;
            _logger = logger;

        }

        public async Task<OperationResult<List<Appointment>>> GetAllAppointments(int page, int perPage)
        {
            try
            {
                //נחזיר  רק את הפגישות מהיום והלאה.. פגשיות עתידיות בלבד
                DateTime now = DateTime.Now;
                List<Appointment> appointments = await _barbershopContext.Appointments
                    .Where(a => a.AppointmentDate > now)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .AsNoTracking()
                    .ToListAsync();
                return new OperationResult<List<Appointment>>(true, StatusCodes.Status200OK, appointments);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error from GetAllAppointments in AppointmentDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<List<Appointment>>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");

            }
        }


        public async Task<OperationResult<Appointment>> CreateNewAppointment(AppointmentToAddDTO newAppointmentDate, int userId)
        {
            try
            {
                // הבדיקה לתקינות התאריך והשעה נמצאים בולידטור

                // הגדרת טווח זמן של חצי שעה לפני ואחרי הפגישה החדשה
                DateTime startTime = newAppointmentDate.AppointmentDate.AddMinutes(-30);
                DateTime endTime = newAppointmentDate.AppointmentDate.AddMinutes(30);

                // בדיקה שאין פגישה קיימת בטווח של חצי שעה
                var isAppointmentExists = await _barbershopContext
                    .Appointments
                    .AnyAsync(x => x.AppointmentDate >= startTime && x.AppointmentDate <= endTime);

                // אם קיימת פגישה בטווח הזמן הזה
                if (isAppointmentExists)
                {
                    _logger.LogError("The date or time is no longer available.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status409Conflict, "התאריך או השעה שנבחרו תפוסים, בחר מועד אחר");
                }

                // המרת האובייקט שהתקבל לאובייקט רגיל של "תור"
                Appointment appointmentMapped = _mapper.Map<Appointment>(newAppointmentDate);
                if (appointmentMapped == null)
                {
                    _logger.LogError("Unable to convert object from type `AppointmentToAddDTO` to type `Appointment` .");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "שגיאת מערכת");
                }

                // נוסיף לו את המספר המזהה של המשתמש
                appointmentMapped.UserId = userId;


                // נוסיף את הפגישה למסד הנתונים
                _barbershopContext
                    .Appointments
                    .Add(appointmentMapped);

                // נשמור את השינויים בטבלה
                await _barbershopContext.SaveChangesAsync();



                return new OperationResult<Appointment>(true, StatusCodes.Status200OK, appointmentMapped);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from CreateNewAppointment in AppointmentDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<Appointment>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");
            }
        }

        public async Task<OperationResult<Appointment>> UpdateAppointmentDate(int appointmentId, AppointmentToAddDTO newAppointmentDate, int userId)
        {
            try
            {
                Appointment appointmentOfUserInDB = _barbershopContext
                    .Appointments
                    .FirstOrDefault(x => x.AppointmentId == appointmentId && x.UserId == userId);

                // אם לא נמצאה פגישה בתאריך הזה
                if (appointmentOfUserInDB == null)
                {
                    _logger.LogError("There is no appointment on this date for this user.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status404NotFound, "אין תור קיים בתאריך זה למשתמש זה");
                }

                // בדיקה שאין פגישה אחרת בשעה הזאת
                var startTime = newAppointmentDate.AppointmentDate.AddMinutes(-30);
                var endTime = newAppointmentDate.AppointmentDate.AddMinutes(30);

                var isAppointmentExists = await _barbershopContext
                    .Appointments
                    .AnyAsync(x => x.AppointmentId != appointmentId && x.AppointmentDate >= startTime && x.AppointmentDate <= endTime);

                // אם קיימת פגישה בטווח הזמן הזה
                if (isAppointmentExists)
                {
                    _logger.LogError("The date and time are not available.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status409Conflict, "התאריך או השעה שנבחרו תפוסים, בחר מועד אחר");
                }

                // אם הפגישה נמצאת ואין פגישה אחרת בזמן החדש
                // עדכון הפגישה לתאריך ולשעה החדשים
                appointmentOfUserInDB.AppointmentDate = newAppointmentDate.AppointmentDate;

                // שמירת השינויים במסד הנתונים
                _barbershopContext.SaveChanges();

                //שליפת פרטי הפגישה עם הנתונים החדשים
                Appointment updateAppointmentFromDB = _barbershopContext
                    .Appointments
                    .FirstOrDefault(x => x.AppointmentId == appointmentId);
                if (updateAppointmentFromDB == null)
                {
                    _logger.LogError($"Can't update an appointment in DB");

                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "לא מצליח לעדכן פרטי פגישה");

                }

                return new OperationResult<Appointment>(true, StatusCodes.Status200OK, updateAppointmentFromDB);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from UpdateAppointment in AppointmentDL class, Message: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}");
                return new OperationResult<Appointment>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");
            }
        }


        public async Task<OperationResult<Appointment>> DeleteAppointment(int appointmentId, int userId)
        {
            try
            {
                //נחפש את הפגישה שנרצה למחוק, ונוודא שהמספר המזהה שרוצה למחוק את הפגישה הוא בעל הפגישה
                Appointment appointmentFromDB = await _barbershopContext
                    .Appointments
                    .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId && x.UserId == userId);

                //אם לא קיימת פגישה ליוזר 
                if (appointmentFromDB == null)
                {
                    _logger.LogError("There is no appointment to delete.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status404NotFound, "  לא קיימת במערכת פגישה למחיקה");
                }
                //נמחק את הפגישה מבוקשת
                _barbershopContext.Appointments.Remove(appointmentFromDB);
                _barbershopContext.SaveChanges();

                //נוודא מחיקה של הפגישה
                Appointment searchAppointmentInDB = await _barbershopContext
                .Appointments
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId && x.UserId == userId);
                if (searchAppointmentInDB != null)
                {
                    _logger.LogError("Can't delete appointment in DB.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, " לא מצליח למחוק את הפגישה");
                }

                return new OperationResult<Appointment>(true, StatusCodes.Status200OK, searchAppointmentInDB);

            }

            catch (Exception ex)
            {
                _logger.LogError($"Error from DeleteAppointment in  AppointmentDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<Appointment>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");
            }


        }


        public async Task<OperationResult<int>> GetFutureAppointmentsCount()
        {
            try
            {

                DateTime now = DateTime.UtcNow;
                var futureAppointmentsCount = await _barbershopContext
                    .Appointments
                    .Where(a => a.AppointmentDate > now)
                    .CountAsync();
                if (futureAppointmentsCount == 0)
                {
                    _logger.LogError("There is no future appointments.");
                    return new OperationResult<int>(true, StatusCodes.Status200OK, 0, "אין פגישות עתידיות");

                }
                return new OperationResult<int>(true, StatusCodes.Status200OK, futureAppointmentsCount);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error from GetFutureAppointmentsCount in  AppointmentDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<int>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");
            }
        }
    }

}





