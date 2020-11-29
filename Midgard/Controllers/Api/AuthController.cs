using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.DbModels;
using Midgard.Enumerates;
using Midgard.Models.Api.Auth;
using Midgard.Utilities;
using Midgard.ViewModels.Api.Shared;
using reCAPTCHA.AspNetCore;

namespace Midgard.Controllers.Api
{
    [ApiController]
    [Route("Api/[controller]")]
    public class AuthController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        private IRecaptchaService Recaptcha { get; }

        public AuthController(IConfiguration config, MidgardContext context, IRecaptchaService recaptchaService)
        {
            Config = config;
            Db = context;
            Recaptcha = recaptchaService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Todo: Uncomment recaptcha verify.
            // var recaptcha = await Recaptcha.Validate(model.Recaptcha);
            // if (!recaptcha.success)
            // {
            //     return new JsonResult(new ErrorViewModel()
            //     {
            //         Message = new()
            //         {
            //             Message = "message.fatal.recaptcha_failed"
            //         }
            //     })
            //     {
            //         StatusCode = 403
            //     };
            // }

            var isExists = Db.Users.Any(u => u.Username == model.Username || u.Email == model.Email);
            if (isExists)
            {
                return new JsonResult(new ErrorViewModel()
                {
                    Message = new()
                    {
                        Message = "message.fatal.username_or_email_exists"
                    }
                })
                {
                    StatusCode = 403
                };
            }

            var now = DateTime.Now;
            var salt = Time.GetSaltByTime(now);
            var passwordHashed = Passwords.Hash(model.Password, salt);

            var user = new User()
            {
                Id = Guid.NewGuid(), 
                Email = model.Email, 
                Username = model.Username, 
                Password = passwordHashed,
                PasswordSalt = salt, 
                Permission = Permission.User, 
                IsEmailVerified = false,
                TryTimes = 0, 
                CoolDownLevel = 0, 
                CoolDownEndTime = now
            };

            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();
            
            HttpContext.Session.SetString("LoggedUserId", user.Id.ToString("N"));

            return new JsonResult(new BoolViewModel(true));
        }

        [HttpGet]
        [Route("[action]/{username}")]
        public IActionResult CheckUsername([FromRoute] string username)
        {
            var result = Db.Users.Any(u => u.Username == username);
            return new JsonResult(new BoolViewModel(result));
        }
        
        [HttpGet]
        [Route("[action]/{email}")]
        public IActionResult CheckEmail([FromRoute] [EmailAddress] string email)
        {
            var result = Db.Users.Any(u => u.Email == email);
            return new JsonResult(new BoolViewModel(result));
        }
    }
}