using Azure.Core;
using BarbershopBL.Interfaces;
using BarbershopDL.EF.Models;
using BarbershopDL.Interfaces;
using BarbershopDL.Services;
using BarbershopEntities;
using BarbershopEntities.DTO.AppintmentDTO;
using FluentNHibernate.Automapping;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BarbershopBL.Services
{
    public class AppointmentBL : IAppointmentBL
    {
        private readonly IAppointmentDL _appointmentDL;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AppointmentBL> _logger;


        public AppointmentBL(IAppointmentDL appointmentDL, IHttpContextAccessor httpContextAccessor, ILogger<AppointmentBL> logger)
        {
            _appointmentDL = appointmentDL;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<OperationResult<List<Appointment>>> GetAllAppointments(int page, int perPage)
        {
            try
            {
                //נבדוק אם יש בכלל תורים קיימים במערכת
                var AllAppointment = await _appointmentDL.GetAllAppointments(page, perPage);
                //אם יש תורים קיימים
                if (AllAppointment.IsSuccessful)
                {
                    //נחזיר את כל התורים הקיימים (העתידיים בלבד).
                    return new OperationResult<List<Appointment>>(true, StatusCodes.Status200OK, AllAppointment.MyObject);
                }

                _logger.LogError("There are no appointment yet.");
                return new OperationResult<List<Appointment>>(false, StatusCodes.Status404NotFound, "אין תורים עתידיים");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetAllAppointments in `AppointmentBL` class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<List<Appointment>>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");
            }


        }

        public async Task<OperationResult<Appointment>> CreateNewAppointment(AppointmentToAddDTO newAppointmentDate)
        {
            try
            {
                //אם לא קיבלנו כלום מהקליינט או שקיבלנו = פרמטר שאינו יכול להיות מומר  לדייט טיים
                if (newAppointmentDate == null || newAppointmentDate.AppointmentDate == default)
                {
                    _logger.LogError("Invalid date");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "תאריך שהתקבל לא חוקי");
                }
                // בדיקה שהתור לא נופל על יום שבת
                //אם התאריך הוא יום שבת
                if (IsSaturday(newAppointmentDate.AppointmentDate))
                {
                    _logger.LogError("The date or time is no longer available, Closed on Saturday.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "המספרה סגורה בשבת");

                }

                //נשלוף את המספר המזהה מהטוקן
                int userIdInt = ParseUserIdFromToken().MyObject;

                //נשלח את הפגישה המיועדת לשכבת הדטה בייס
                OperationResult<Appointment> res = await _appointmentDL.CreateNewAppointment(newAppointmentDate, userIdInt);

                return res;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error from CreateNewAppointment in AppointmentBL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<Appointment>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");


            }
        }

        public async Task<OperationResult<Appointment>> UpdateAppointmentDate(string appointmentId, AppointmentToAddDTO newAppointmentDate)
        {
            try
            {
                if (newAppointmentDate == null || newAppointmentDate.AppointmentDate == default)
                {
                    _logger.LogError("Invalid date");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "תאריך שהתקבל לא חוקי");
                }

                //נוודא שהתאריך והשעה שהתקבלו לא נופלים על יום שבת
                if (IsSaturday(newAppointmentDate.AppointmentDate))
                {
                    _logger.LogError("The date or time is no longer available, Closed on Saturday.");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "המספרה סגורה בשבת");

                }

                //ננסה להמיר את המספר המזהה שקיבלנו לאינט
                //נבדוק אם המספר המזהה של הפגישה תקין
                if (!int.TryParse(appointmentId, out var appId) || appId < 0)
                {
                    _logger.LogError("Invalid AppointmentID");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "מספר תור מזהה לא חוקי");
                }
                int appointmentIdInt = int.Parse(appointmentId);



                //נשלוף את המספר המזהה מהטוקן
                int userIdInt = ParseUserIdFromToken().MyObject;
                //נשלח את הפגישה המיועדת לשכבת הדטה בייס
                OperationResult<Appointment> res = await _appointmentDL.UpdateAppointmentDate(appointmentIdInt, newAppointmentDate, userIdInt);


                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from UpdateAppointment in AppointmentBL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<Appointment>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");

            }


        }


        public async Task<OperationResult<Appointment>> DeleteAppointment(string appointmentId)
        {
            try
            {
                //ננסה להמיר את המספר המזהה שקיבלנו לאינט
                //נבדוק אם המספר המזהה של הפגישה תקין
                if (!int.TryParse(appointmentId, out var appId) || appId < 0)
                {
                    _logger.LogError("Invalid AppointmentID");
                    return new OperationResult<Appointment>(false, StatusCodes.Status400BadRequest, "מספר מזהה של תור לא חוקי");
                }
                int appointmentIdInt = int.Parse(appointmentId);


                //נשלוף את המספר המזהה מהטוקן
                int userIdInt = ParseUserIdFromToken().MyObject;

                //נשלח את המספר המזהה של הפגישה למחיקה לשכבת הדטה בייס
                OperationResult<Appointment> res = await _appointmentDL.DeleteAppointment(appointmentIdInt, userIdInt);

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from UpdateAppointment in AppointmentBL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<Appointment>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");

            }


        }

        public async Task<OperationResult<int>> GetFutureAppointmentsCount()
        {
            try
            {
                var res = await _appointmentDL.GetFutureAppointmentsCount();
                return res;


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetFutureAppointmentsCount in AppointmentBL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<int>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");

            }
        }



        private bool IsSaturday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday;
        }

        public OperationResult<int> ParseUserIdFromToken()
        {
            //נשלוף את הטוקן
            var token = _httpContextAccessor.HttpContext.Request.Cookies[CookiesKeys.AccessToken];
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token is missing.");
                return new OperationResult<int>(false, StatusCodes.Status401Unauthorized, "חסר טוקן");
            }

            //נוציא מתוכו את המספר המזהה של הלקוח
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            string userId = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogError("User ID is null.");
                return new OperationResult<int>(false, StatusCodes.Status401Unauthorized, "מספר מזהה של משמש ריק");
            }
            else if (!int.TryParse(userId, out var id))
            {
                _logger.LogError("Invalid user ID.");
                return new OperationResult<int>(false, StatusCodes.Status400BadRequest, "מספר מזהה של משתמש לא חוקי");
            }
            //נמיר, ונשלח את המספר המזהה של הלקוח שקבע את התור

            int userIdInt = int.Parse(userId);
            //נשלח את הפגישה המיועדת לשכבת הדטה בייס

            return new OperationResult<int>(true, StatusCodes.Status200OK, userIdInt);
        }

    }
}


