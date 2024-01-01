using Api.DTOs.Account;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AccountController(JWTService jwtService,SignInManager<User> signInManager,UserManager<User> userManager,EmailService emailService,IConfiguration config)
        {
            _jwtService = jwtService;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _config = config;
        }

        [Authorize]
        [HttpGet("refresh-user-token")]

        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }


        // POST: api/login
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
          
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                return Unauthorized("Invalid UserName  Or PAssword");
            }

            if(user.EmailConfirmed == null)
            {
                return Unauthorized("Please Confirm Your Email");
            }

            var result = await  _signInManager.CheckPasswordSignInAsync(user,model.Password,false);

            if(!result.Succeeded)
            {
                return Unauthorized("Password Is not Correct.Please Check It Again!!");
            }
            return CreateApplicationUserDto(user);
        }

        // POST: api/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto  model)
        {
            if (await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest($"An Existing Accounts is using {model.Email},email Address.Please Try With Another Email");
            }

            var userToAdd = new User 
            { 
                FirstName =model.FirstName.ToLower(),
                LastName =model.LastName.ToLower(),
                UserName= model.Email.ToLower(),
                Email=model.Email.ToLower(),

            };

            var result = await _userManager.CreateAsync(userToAdd,model.Password);

            if(!result.Succeeded) {
                return BadRequest(result.Errors);
            }


            try
            {
                if (await SendConfirmEmailAsync(userToAdd))
                {
                    return Ok(new JsonResult(new { title = "Account Created", message = "Your Account has been created ! Please Confirm Your Email Address" }));
                }
                return BadRequest("Failed To Send Email ! Please Contact Admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed To Send Email ! Please Contact Admin");
            }

        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) { 
                return Unauthorized("This Email address Has not been Yet Registered yet");
            }

            if(user.EmailConfirmed == true)
            {
                return BadRequest("Your Email Was Confirmed Before.Please login To your Account ");
            }

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if(result.Succeeded)
                {
                    return Ok(new JsonResult(new {title = "Email Confirmed",message ="Your Email address is Confirmed.You can Login Now"}));
                }
                return BadRequest("Invalid Token. Please Try Again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid Token. Please Try Again");
            }
        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendEmailConfiirmationLink(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid Email ");
            }
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) {
                return Unauthorized("This email Address has not been Registered Yet");
            }
            if (user.EmailConfirmed == true)
            {
                return BadRequest("Your Email Was Confirmed Before.Please login To your Account ");
            }

            try
            {
                if(await SendConfirmEmailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Confirmation Link Sent ", message = " Please Confirm Your Email Address!!" }));
                }

                return BadRequest("Failed To Send Email ! Please Contact Admin");

            }
            catch (Exception)
            {

                return BadRequest("Failed To Send Email ! Please Contact Admin");
            }


        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async  Task<IActionResult> ForgotUserNameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid Email ");
            }
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized("This email Address has not been registered yet");
            }

            if (user.EmailConfirmed == false)
            {
                return BadRequest("Please confirm Your Email Address First ");
            }

            try
            {
                if(await SendForgotUserNameOrPasswordEmail(user))
                {
                    return Ok(new JsonResult(new { title = "Forgot UserName or Password Email Sent", message = " Please Check Your Email !!" }));
                }
                return BadRequest("Failed To Send Email ! Please Contact Admin");
            }
            catch(Exception)
            {
                return BadRequest("Failed To Send Email ! Please Contact Admin");
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult>ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized("This Email Address has not been Registered Yet");
            }

            if (user.EmailConfirmed == false)
            {
                return BadRequest("Please confirm Your Email Address First ");
            }
            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Password Reset Success", message = "Your Password has been Reset" }));
                }
                return BadRequest("Invalid Token. Please Try Again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid Token. Please Try Again");
            }

        }

        #region Private Helper Methods
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWT(user),
            };
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email.ToLower());
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ValidAudience"]}/{_config["Email:ConfirmationEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
                "<p> PLease  Confirm your email address by clicking on the following link.</p>" +
                $"<p><a href =\"{url}\"> Click here </p>" +
                "<p>Thank You,</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm Your Email", body);

            return await _emailService.SendEmailAsync(emailSend);

        }

        private async Task<bool> SendForgotUserNameOrPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ValidAudience"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
               $"<p>Username : {user.UserName}</p>" +
               "<p>In Order to reset the Password , please click on the following link!!</p>" +
               $"<p><a href =\"{url}\"> Click here </p>" +
               "<p>Thank You,</p>" +
               $"<br>{_config["Email:ApplicationName"]}";


            var emailSend = new EmailSendDto(user.Email, "Forgot Username Or Password", body);

            return await _emailService.SendEmailAsync(emailSend);

        }
        #endregion
    }
}
