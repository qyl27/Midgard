using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.DbModels;
using Midgard.Enumerates;
using Midgard.Models.Yggdrasil.SessionServer;
using Midgard.Utilities;
using Midgard.ViewModels.Yggdrasil.Shared;

namespace Midgard.Controllers.Yggdrasil
{
    [ApiController]
    [Route("Yggdrasil/SessionServer/Session/Minecraft")]
    public class SessionServerController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        
        public SessionServerController(IConfiguration config, MidgardContext context)
        {
            Config = config;
            Db = context;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Join([FromBody] JoinModel model)
        {
            var token = (from t in Db.Tokens
                where t.AccessToken == model.AccessToken
                      && t.Status != TokenStatus.Invalid
                select t).FirstOrDefault();

            if (token == null)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid token.")) { StatusCode = 403 };
            }

            if (token.BindProfile.Id != model.SelectedProfile)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid token.")) { StatusCode = 403 };
            }

            var expireTime = Config.GetSection("Yggdrasil:Sessions:ExpireTime").Get<int>();
            Db.Sessions.Add(new Session
            {
                AccessToken = model.AccessToken,
                BindProfile = token.BindProfile,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.MapToIPv4(),
                ServerId = model.ServerId,
                ExpireTime = DateTime.Now.AddSeconds(expireTime)
            });
            Db.SaveChanges();
            
            return NoContent();
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult HasJoined([FromQuery] HasJoinedModel model)
        {
            var profile = (from p in Db.Profiles
                where p.Name == model.Username
                select p).FirstOrDefault();
            if (profile == null)
            {
                return NoContent();
            }

            foreach (var session in profile.ActiveSessions
                .Where(session => session.ServerId == model.ServerId))
            {
                if (model.IP != null && model.IP.Equals(session.ClientIp))
                {
                    return new JsonResult(InformationSerializer.ProfileSerializer(profile, 
                        profile.Skin, profile.Cape, true));
                }

                if (model.IP == null)
                {
                    return new JsonResult(InformationSerializer.ProfileSerializer(profile, 
                        profile.Skin, profile.Cape, true));
                }
            }

            return NoContent();
        }
        
        [HttpGet]
        [Route("[action]/{uuid}")]
        public IActionResult Profile([FromQuery] ProfileModel model, [FromRoute] Guid uuid)
        {
            var profile = (from p in Db.Profiles
                where p.Id == uuid
                select p).FirstOrDefault();
            if (profile == null)
            {
                return StatusCode(204);
            }

            return new JsonResult(
                InformationSerializer.ProfileSerializer(profile, profile.Skin, profile.Cape, !model.Unsigned));
        }
    }
}