using BarbershopBL.Interfaces;
using BarbershopDL.EF.Models;
using BarbershopEntities;
using BarbershopEntities.DTO.AppintmentDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarbershopApi.Controllers
{
    [Authorize]

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentBL _appointmentBL;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IAppointmentBL appointmentBL, ILogger<AppointmentController> logger)
        {
            _appointmentBL = appointmentBL;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointments(int page = 1, int perPage = 10) //הצגת כל התורים הקיימים במערכת
        {
            try
            {

                var res = await _appointmentBL.GetAllAppointments(page,perPage);
                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from GetAllAppointments in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode, res.Message);
                }

                return Ok(res.MyObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetAllAppointments, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewAppointment([FromBody]AppointmentToAddDTO dateAndTime) // קביעת תור חדש
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // אם המודל לא תקין, מחזירים שגיאה ללקוח עם פרטי השגיאות
                    _logger.LogError($"Error from validation UserToAddValidatorDTO{ModelState}");
                    return BadRequest(ModelState);
                }
                var res = await _appointmentBL.CreateNewAppointment(dateAndTime);
                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from CreateNewAppointment in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode,res.Message);
                }
                return StatusCode(res.StatusCode, res.MyObject);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from CreateNewAppointment, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{appointmentId?}")]

        public async Task<IActionResult> UpdateAppointmentDate([FromRoute] string appointmentId, [FromBody] AppointmentToAddDTO updateAppointment) // עדכון תור קיים
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // אם התאריך לא תקין, מחזירים שגיאה ללקוח עם פרטי השגיאות
                    _logger.LogError($"Error from validation AppointmentUpdateDTOValidator{ModelState}");
                    return BadRequest(ModelState);
                }

                var res = await _appointmentBL.UpdateAppointmentDate(appointmentId, updateAppointment);
                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from UpdateAppointment in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode,res.Message);
                }
                return StatusCode(res.StatusCode, res.MyObject);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from UpdateAppointment, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpDelete("{appointmentId?}")]
        public async Task<IActionResult> DeleteAppointment([FromRoute] string appointmentId) //מחיקת תור קיים
        {
            try
            {
                var res = await _appointmentBL.DeleteAppointment(appointmentId);

                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from DeleteAppointment in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode, res.Message);
                }
                return StatusCode(res.StatusCode, res.MyObject);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from DeleteAppointment, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<ActionResult<int>> GetFutureAppointmentsCount()
        {
            try
            {
                var res = await _appointmentBL.GetFutureAppointmentsCount();
                if (!res.IsSuccessful)
                {
                    _logger.LogError("Can't count num of appointment in server");
                    return StatusCode(res.StatusCode, res.Message);
                }
                return StatusCode(res.StatusCode,res.MyObject);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetFutureAppointmentsCount, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
    }

}
