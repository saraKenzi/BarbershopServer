using BarbershopBL.Interfaces;
using BarbershopBL.Services;
using BarbershopDL.EF.Models;
using BarbershopEntities;
using BarbershopEntities.DTO.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BarbershopApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<AppointmentController> _logger;
        private readonly IUserBL _userBL;
        public UserController(IUserBL userBL, ILogger<AppointmentController> logger)
        {
            _userBL = userBL;
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserToAddDTO newUser) //הוספת משתמש חדש
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // אם המודל לא תקין, מחזירים שגיאה ללקוח עם פרטי השגיאות
                    _logger.LogError($"Error from validation UserToAddValidatorDTO{ModelState}");
                    return BadRequest(ModelState);
                }

                var res = await _userBL.AddUser(newUser);

                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from AddUser in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode, res.Message);
                }
                
                return StatusCode(res.StatusCode,res.MyObject);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from AddUser, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
        {
            try
            {
                //אם מה שנשלח לא תקין נחזיר קוד 400

                if (!ModelState.IsValid)
                {
                    // אם המודל לא תקין, מחזירים שגיאה ללקוח עם פרטי השגיאות
                    _logger.LogError($"Error from validation UserLoginDTOValidator{ModelState}");
                    return BadRequest(ModelState);
                }
                var res = await _userBL.Login(userLoginDTO);
                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from Login in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode, res.Message);
                }
                return StatusCode(res.StatusCode, res.MyObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from Login, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }


        }

        [HttpGet]
        public IActionResult Logout()
        {
            try
            {
               if (!_userBL.Logout())
                    return BadRequest();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from Logout, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet("{userId?}")]
        public async Task< IActionResult> GetUserDetailsByUserId([FromRoute] string userId)
        {
            try
            {
                var res = await _userBL.GetUserDetailsByUserId(userId);
                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from GetUserDetailsByUserId in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode, res.Message);
                }
                    return StatusCode(res.StatusCode, res.MyObject);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Error from GetUserDetailsByUserId, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{userName?}")]

        public async Task<IActionResult> GetUserIdByFirstName([FromRoute] string userName)
        {
            try
            {
                var res = await _userBL.GetUserIdByFirstName(userName);
                if (!res.IsSuccessful)
                {
                    _logger.LogError($"Error from GetUserIdsByFirstName in controller{res.StatusCode}");
                    return StatusCode(res.StatusCode, res.Message);
                }
                return StatusCode(res.StatusCode, res.MyObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetUserIdsByFirstName, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


    }
}
