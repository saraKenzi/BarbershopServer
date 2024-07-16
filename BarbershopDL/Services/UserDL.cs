using AutoMapper;
using BarbershopDL.EF.Contexts;
using BarbershopDL.EF.Models;
using BarbershopDL.Interfaces;
using BarbershopEntities;
using BarbershopEntities.DTO.UserDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopDL.Services
{
    public class UserDL:IUserDL
    {
        private readonly BarbershopContext _barbershopContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserDL> _logger;
        public UserDL(BarbershopContext barbershopContext,IMapper mapper,ILogger<UserDL> logger)
        {
            _barbershopContext = barbershopContext;
            _mapper = mapper;
            _logger = logger;
        }

        public  async Task<OperationResult<UserToResponseDTO>> AddUser(UserToAddDTO newUser)

        {
            try
            {
                User searchUserInDB = await _barbershopContext.Users
                        .AsNoTracking()
                     .FirstOrDefaultAsync(u => u.UserName == newUser.UserName);

                //אם כבר קיים כזה שם משתמש א"א ליצור את המשתמש 
                if (searchUserInDB != null)
                {
                    _logger.LogError("There is already a user with that name.");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status409Conflict," קיים משתמש עם שם זהה, בחר שם משתמש אחר");
                }

                // הצפנת הסיסמה לפני האחסון
                string hashedPassword = HashPassword(newUser.Password);
                if (hashedPassword == null)
                {
                    _logger.LogError("Unable to encrypt the password.");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status400BadRequest,"שגיאת מערכת");

                }
                // עדכון הסיסמא המוצפנת למשתמש
                newUser.Password = hashedPassword;
                //המרת המשתמש למשתמש רגיל - כמו בטבלה
                User userMap = _mapper.Map<User>(newUser);
                if (userMap == null)
                {
                    _logger.LogError("Unable to convert object from type `UserToAddDTO` to type `User` .");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status400BadRequest, "שגיאת מערכת");
                }

                //הכנסת המשתמש למסד הנתונים
                _barbershopContext.Users.Add(userMap);
                _barbershopContext.SaveChanges();

                //נשלוף את כל המשתמש שנוצר לנו לתוך משתנה
                User userCreated = _barbershopContext.Users
                   .FirstOrDefault(u => u.UserName == userMap.UserName);

                //אם לא נוצר משתמש 
                if (userCreated == null)
                {
                    _logger.LogError("Error saving to database.");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status500InternalServerError, "לא מצליח לשמור נתונים במערכת");
                }
                //במקרה שכן נוצר משתמש חדש נמחק את הסיסמא 
                //userCreated.Password = null;
                UserToResponseDTO userToResponse = _mapper.Map<UserToResponseDTO>(userCreated);
              
                if (userToResponse == null)
                {
                    _logger.LogError("Unable to convert object from type `User` to type `UserToResponseDTO` .");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status400BadRequest,"שגיאת מערכת");
                }

                //נשלח לשכבת הביאל את המשתמש החדש שנוצר
                return new OperationResult<UserToResponseDTO>(true, StatusCodes.Status200OK, userToResponse);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from AddUser in UserDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status500InternalServerError,"שגיאת מערכת");
            }

        }

        public async Task<OperationResult<UserToResponseDTO>> Login(UserLoginDTO user)
        {
            try
            {
                //נחפש משתמש זהה עם אותו שם משתמש
                User userFromDB = _barbershopContext
                     .Users
                     .FirstOrDefault(u => u.UserName == user.UserName);

                //אם הוא לא קיים נחזיר לא קיים כזה שם משתמש
                if (userFromDB == null)
                {
                    _logger.LogError("Username does not exist.");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status404NotFound,"מצטערים, לא קיים משתמש בשם זה, נסה שנית");
                }
                //אחר כך נבדוק את הסיסמא אם היא תקינה
                bool isPasswordValid = VerifyPassword(user.Password, userFromDB.Password);

                //אם לא נחזיר שגיאת התאמה
                if (!isPasswordValid)
                {
                    _logger.LogError("Incorrect password.");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status401Unauthorized,"סיסמא לא תקינה, נסה שנית!");
                }
                //ע"מ לשלוח את המשתמש על כל פרטיו נמחק לו את הסיסמא כדי לא לשלוח אותה לקליינט
                UserToResponseDTO userToResponse = _mapper.Map<UserToResponseDTO>(userFromDB);
                if (userToResponse == null)
                {
                    _logger.LogError("Unable to convert object from type `User` to type `UserToResponseDTO` .");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status400BadRequest,"שגיאת מערכת");
                }


                return new OperationResult<UserToResponseDTO>(true, StatusCodes.Status200OK, userToResponse);

            }
            catch (Exception ex) {
                _logger.LogError($"Error from Login in UserDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status500InternalServerError, "שגיאת מערכת");

            }

        }

        public async Task<OperationResult<UserToResponseDTO>> GetUserDetailsByUserId(int userId)
        {
            try
            {
                //נחפש במסד הנתונים משתמש עם מספר מזהה זהה למספר המזהה שהתקבל
                User searchUserById = await _barbershopContext.Users
                        .AsNoTracking()
                     .FirstOrDefaultAsync(u => u.UserId == userId);

                //אם לא מצאנו משתמש עם מספר מזהה זהה למה שהתקבלל מהקליינט
                if (searchUserById == null)
                {
                    _logger.LogError("There is no user with that Id.");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status404NotFound,"משתמש לא קיים");
                }

                //נמיר את המשתמש למשתמש בלי סיסמא
                UserToResponseDTO userToResponse = _mapper.Map<UserToResponseDTO>(searchUserById);

                if (userToResponse == null)
                {
                    _logger.LogError("Unable to convert object from type `User` to type `UserToResponseDTO` .");
                    return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status400BadRequest, "שגיאת מערכת");
                }

                //נשלח לשכבת הביאל את המשתמש החדש שנוצר
                return new OperationResult<UserToResponseDTO>(true, StatusCodes.Status200OK, userToResponse);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetUserDetailsByUserId in UserDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<UserToResponseDTO>(false, StatusCodes.Status500InternalServerError,"שגיאת מערכת");
            }
        }
        public async Task <OperationResult<List<UserToResponseDTO>>> GetUserIdByFirstName(string userName)
        {
            try
            {
                //נחפש במסד הנתונים משתמש עם שם או שם משפחה שמכילים את המחרוזת שנשלחה
                List<User> searchUser = await _barbershopContext.Users
                        .AsNoTracking()
                     .Where(u => u.FirstName.Contains(userName) || u.LastName.Contains(userName))
                     .ToListAsync();

                //אם לא מצאנו משתמש עם שם פרטי או שם משפחה
                if (searchUser.Count==0)
                {
                    _logger.LogError("There is no user with that name.");
                    return new OperationResult<List<UserToResponseDTO>>(false, StatusCodes.Status404NotFound,"משתמש לא קיים");
                }

                //נמפה את המשתמשים ששלפנו למשתמשים בלי סיסמא
                List<UserToResponseDTO> userToResponse = searchUser
                    .Select(u => _mapper.Map<UserToResponseDTO>(u))
                    .ToList();

                if (userToResponse.Count==0)
                {
                    _logger.LogError("Unable to convert object from type `User` to type `UserToResponseDTO` .");
                    return  new OperationResult<List<UserToResponseDTO>>(false, StatusCodes.Status400BadRequest, "שגיאת מערכת");
                }

                //נשלח לשכבת הביאל את המשתמש החדש שנוצר
                return new OperationResult<List<UserToResponseDTO>>(true, StatusCodes.Status200OK, userToResponse);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error from GetUserDetailsByUserId in UserDL class, Message: {ex.Message}," +
                $" InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace} ");
                return new OperationResult<List<UserToResponseDTO>>(false, StatusCodes.Status500InternalServerError,"שגיאת מערכת");
            }
        }






        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

       
        private bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
        }

    }
}
