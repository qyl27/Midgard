using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Midgard.DbModels;
using Midgard.Models.Api.Profile;
using Midgard.Utilities;
using Midgard.ViewModels.Api.Shared;
using reCAPTCHA.AspNetCore;

namespace Midgard.Controllers.Api
{
    [ApiController]
    [Route("Api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        private IRecaptchaService Recaptcha { get; }

        public ProfileController(IConfiguration config, MidgardContext context, IRecaptchaService recaptchaService)
        {
            Config = config;
            Db = context;
            Recaptcha = recaptchaService;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create([FromBody] CreateModel model)
        {
            #region Check sessions.

            var uid = HttpContext.Session.GetString(AuthController.SessionKey);
            if (string.IsNullOrWhiteSpace(uid))
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.not_logged"
                    }
                }) {StatusCode = 401};
            }
            var user = Db.Users.FirstOrDefault(u => u.Id == Guid.Parse(uid));
            if (user is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.not_logged"
                    }
                }) {StatusCode = 401};
            }

            #endregion
            
            var result = Db.Profiles.Any(p => p.Name == model.Name);
            if (result)
            {
                return new JsonResult(new ErrorViewModel()
                {
                    Message = new()
                    {
                        Message = "message.error.profile_exists"
                    }
                }) {StatusCode = 403};
            }

            var profilesLimit = Config.GetSection("General:MaxProfileCount").Get<int>();
            if (profilesLimit > 0 && user.Profiles.Count >= profilesLimit)
            {
                return new JsonResult(new ErrorViewModel()
                {
                    Message = new()
                    {
                        Message = "message.error.profile_limit_reached"
                    }
                }) {StatusCode = 403};
            }

            #region Do add profile.

            user.Profiles.Add(new Profile
            {
                Id = Uuid.GetOfflinePlayerUuid(model.Name), 
                Name = model.Name, 
                IsSelected = false, 
                Owner = user
            });
            Db.SaveChanges();

            #endregion

            return new JsonResult(new BoolViewModel(true));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ProfileRemain()
        {
            #region Check sessions.

            var uid = HttpContext.Session.GetString(AuthController.SessionKey);
            if (string.IsNullOrWhiteSpace(uid))
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.not_logged"
                    }
                }) {StatusCode = 401};
            }
            var user = Db.Users.FirstOrDefault(u => u.Id == Guid.Parse(uid));
            if (user is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.not_logged"
                    }
                }) {StatusCode = 401};
            }

            #endregion
            
            var profilesLimit = Config.GetSection("General:MaxProfileCount").Get<int>();
            if (profilesLimit == 0)
            {
                return new JsonResult(new IntegerViewModel(-1));
            }
            return new JsonResult(new IntegerViewModel(profilesLimit - user.Profiles.Count));
        }
    }
}