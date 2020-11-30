using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            #region Do create profile.

            var isOfflineUuid = Config.GetSection("Yggdrasil:Features:OfflineUuid").Get<bool>();
            var uuid = isOfflineUuid ? Uuid.GetOfflinePlayerUuid(model.Name) : Guid.NewGuid();
            
            user.Profiles.Add(new Profile
            {
                Id = uuid, 
                Name = model.Name, 
                IsSelected = false, 
                Owner = user
            });
            Db.SaveChanges();

            #endregion

            return NoContent();
        }

        [HttpPut]
        [Route("[action]/{profileId}/{skinId}")]
        public IActionResult Skin([FromRoute] Guid profileId, [FromRoute] Guid skinId)
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

            var profile = user.Profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.profile_not_exists"
                    }
                }) {StatusCode = 403};
            }

            var skin = Db.Skins.FirstOrDefault(s => s.Id == skinId);
            if (skin is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.skin_not_exists"
                    }
                }) {StatusCode = 403};
            }

            #region Do attach skin.

            profile.Skin = skin;
            Db.SaveChanges();

            #endregion

            return NoContent();
        }
        
        
        [HttpDelete]
        [Route("[action]/{profileId}")]
        public IActionResult Skin([FromRoute] Guid profileId)
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

            var profile = user.Profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.profile_not_exists"
                    }
                }) {StatusCode = 403};
            }

            #region Do delete skin.

            profile.Skin = null;
            Db.SaveChanges();

            #endregion

            return NoContent();
        }
        
        
        [HttpPut]
        [Route("[action]/{profileId}/{capeId}")]
        public IActionResult Cape([FromRoute] Guid profileId, [FromRoute] Guid capeId)
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

            var profile = user.Profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.profile_not_exists"
                    }
                }) {StatusCode = 403};
            }

            var cape = Db.Capes.FirstOrDefault(c => c.Id == capeId);
            if (cape is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.cape_not_exists"
                    }
                }) {StatusCode = 403};
            }

            #region Do attach cape.

            profile.Cape = cape;
            Db.SaveChanges();

            #endregion

            return NoContent();
        }
        
        
        [HttpDelete]
        [Route("[action]/{profileId}")]
        public IActionResult Cape([FromRoute] Guid profileId)
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

            var profile = user.Profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.profile_not_exists"
                    }
                }) {StatusCode = 403};
            }

            #region Do delete cape.

            profile.Cape = null;
            Db.SaveChanges();

            #endregion

            return NoContent();
        }


        [HttpDelete]
        [Route("[action]/{profileId}")]
        public IActionResult Manage([FromRoute] Guid profileId)
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

            #region Do delete profile.

            var profile = user.Profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile is null)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.profile_not_exists"
                    }
                });
            }

            #endregion

            return NoContent();
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