using AutoMapper;
using Azure;
using BarbershopBL.Interfaces;
using BarbershopDL.EF.Models;
using BarbershopDL.Interfaces;
using BarbershopDL.Services;
using BarbershopEntities;
using BarbershopEntities.DTO.UserDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopBL.Services
{
    public class UserBL : IUserBL
    {
        private readonly IUserDL _userDL;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<UserBL> _logger;


        public UserBL(IUserDL userDL, IOptions<AppSettings> options, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<UserBL> logger)
        {
            _userDL = userDL;
            _appSettings = options.Value;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<OperationResult<UserToResponseDTO>> AddUser(UserToAddDTO newUser)
        {
            var res = await _userDL.AddUser(newUser);
            //אם הוספת המשתמש החדש לדטה בייס הצליחה

            if (res.IsSuccessful)
            {
                //נג'נרט טוקן למשתמש החדש
                CreateUserToken(res.MyObject);
                return res;

            }
            return res;
        }

        public async Task<OperationResult<UserToResponseDTO>> Login(UserLoginDTO userLoginDTO)
        {
            var res = await _userDL.Login(userLoginDTO);
            //אם קיים משתמש כזה
            if (res.IsSuccessful)
            {
                //נג'נרט טןקן למשתמש החדש
                CreateUserToken(res.MyObject);

                return res;
            }
            return res;


        }

        public bool Logout()
        {
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // אם את משתמשת ב-HTTPS
                    SameSite = SameSiteMode.None,
                    Path = "/"
                };

                _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookiesKeys.AccessToken, cookieOptions);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from Logout, Message: {ex.Message}," +
                    $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return false;
            }



        }


       public async Task<OperationResult<UserToResponseDTO>> GetUserDetailsByUserId(string userId)
        {
            // בדיקת תקינות המספר המזהה שהתקבל
            if (!int.TryParse(userId, out var id) || id < 0)
            {
                _logger.LogError("Invalid userId");
                return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status400BadRequest,"מספר מזהה של משתמש לא חוקי");
            }
            //המרת המספר המזהה למספר
            int userIdInt=int.Parse(userId);
            var res = await _userDL.GetUserDetailsByUserId(userIdInt);

         
            return res;
        }


        public async Task<OperationResult<List<UserToResponseDTO>>> GetUserIdByFirstName(string userName)
        {
            var res = await _userDL.GetUserIdByFirstName(userName);
            //אם מצאנו  משתמש בדטה בייס 

            if (res.IsSuccessful)
            {
                //נשלח אותו לקליינט
                return res;

            }
            //אם לא נחזיר שגיאה
            return res;
        }

    



    private void CreateUserToken(UserToResponseDTO user)
        {
            string newAccessToken = GenerateAccessToken(user);
            CookieOptions cookieTokenOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.Now.AddMinutes(_appSettings.Jwt.ExpireMinutes)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(CookiesKeys.AccessToken, newAccessToken, cookieTokenOptions);
        }

        private string GenerateAccessToken(UserToResponseDTO user)
        {
            var jwtSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Jwt.SecretKey));
            var credentials = new SigningCredentials(jwtSecurityKey, SecurityAlgorithms.HmacSha256);
           
            //{ //נכניס לתוך הטוקן: מספר מזהה של לקוח, שם פרטי, שם משפחה וטלפון
            
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            if (!string.IsNullOrEmpty(user.FirstName))
            {
                claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
            }

            if (!string.IsNullOrEmpty(user.Phone))
            {
                claims.Add(new Claim(ClaimTypes.HomePhone, user.Phone));
            }


            var token = new JwtSecurityToken(
                _appSettings.Jwt.Issuer,
                _appSettings.Jwt.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(_appSettings.Jwt.ExpireMinutes),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
