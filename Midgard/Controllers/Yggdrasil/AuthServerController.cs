using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.DbModels;
using Midgard.Enumerates;
using Midgard.Models.Yggdrasil.AuthServer;
using Midgard.Utilities;
using Midgard.ViewModels.Yggdrasil.AuthServer;
using Midgard.ViewModels.Yggdrasil.Shared;

namespace Midgard.Controllers.Yggdrasil
{
    [ApiController]
    [Route("Yggdrasil/[controller]")]
    public class AuthServerController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        
        public AuthServerController(IConfiguration config, MidgardContext context)
        {
            Config = config;
            Db = context;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(403);
            }

            #region Check cooldown and password.

            var isEnableNonEmailLogin = Config.GetSection("Yggdrasil:Features:NonEmailLogin").Get<bool>();
            var isUsername = isEnableNonEmailLogin && !model.Username.Contains("@");
            
            var user = (from u in Db.Users
                where model.Username == (isUsername ? u.Username : u.Email)
                select u).FirstOrDefault();
            if (user == null)
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            var now = DateTime.Now;
            if (user.CoolDownEndTime > now)
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            user.TryTimes++;
            Db.SaveChanges();

            var maxTryTimes = Config.GetSection("Yggdrasil:Security:MaxTryTimes").Get<int>();
            if (user.TryTimes >= maxTryTimes)
            {
                user.TryTimes = 0;
                user.CoolDownLevel++;
                user.CoolDownEndTime = now.AddMinutes(user.CoolDownLevel * user.CoolDownLevel * 5);
                Db.SaveChanges();
                
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            if (user.Password != Passwords.Hash(model.Password, user.PasswordSalt))
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            user.TryTimes = 0;
            user.CoolDownLevel = 0;
            user.CoolDownEndTime = now;
            Db.SaveChanges();
            
            #endregion

            #region Make response.

            var accessToken = Guid.NewGuid();
            var clientToken = model.ClientToken ?? Guid.NewGuid().ToString("N");

            var result = new AuthenticateViewModel
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };

            if (model.RequestUser)
            {
                result.User = InformationSerializer.UserSerializer(user);
            }

            Profile bindProfile = null; 
            foreach (var profile in user.Profiles)
            {
                result.AvailableProfiles.Add(InformationSerializer.ProfileSerializer(profile));

                if (profile.IsSelected)
                {
                    bindProfile = profile;
                    result.SelectedProfile = InformationSerializer.ProfileSerializer(profile);
                }
            }

            var tokenExpireDays = Config.GetSection("Yggdrasil:Security:TokenExpireDays").Get<int>();
            user.Tokens.Add(new Token()
            {
                AccessToken = accessToken, 
                ClientToken = clientToken, 
                Status = TokenStatus.Active, 
                BindUser = user,
                BindProfile = bindProfile, 
                IssueTime = now, 
                ExpireTime = now.AddDays(tokenExpireDays)
            });
            Db.SaveChanges();

            #endregion

            return new JsonResult(result);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Refresh([FromBody] RefreshModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(403);
            }

            #region Check tokens.
            
            var token = (from t in Db.Tokens
                where t.AccessToken == model.AccessToken
                      && (string.IsNullOrWhiteSpace(model.ClientToken) || model.ClientToken == t.ClientToken)
                      && t.Status != TokenStatus.Invalid
                select t).FirstOrDefault();
            
            if (token == null)
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", "Invalid token."));
            }

            token.Status = TokenStatus.Invalid;
            Db.SaveChanges();

            #endregion

            #region Make Response.
           
            var now = DateTime.Now;
            var accessToken = Guid.NewGuid();
            var clientToken = model.ClientToken ?? Guid.NewGuid().ToString("N");
            
            var result = new RefreshViewModel()
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };

            if (model.RequestUser)
            {
                result.User = InformationSerializer.UserSerializer(token.BindUser);
            }
            
            result.SelectedProfile = model.SelectedProfile;

            Profile profile = null;
            if (!Guid.TryParse(model.SelectedProfile.Id, out var profileId))
            {
                profile = (from p in Db.Profiles
                    where p.Id == profileId
                    select p).FirstOrDefault();
            }

            var tokenExpireDays = Config.GetSection("Yggdrasil:Security:TokenExpireDays").Get<int>();
            Db.Tokens.Add(new Token()
            {
                AccessToken = accessToken, 
                ClientToken = clientToken, 
                Status = TokenStatus.Active, 
                BindUser = token.BindUser,
                BindProfile = profile ?? token.BindProfile, 
                IssueTime = now, 
                ExpireTime = now.AddDays(tokenExpireDays)
            });
            Db.SaveChanges();

            #endregion

            return new JsonResult(result);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Validate([FromBody] TokenModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(403);
            }
            
            var token = (from t in Db.Tokens
                where t.AccessToken == model.AccessToken
                      && (string.IsNullOrWhiteSpace(model.ClientToken) || model.ClientToken == t.ClientToken)
                      && t.Status != TokenStatus.Invalid
                select t).FirstOrDefault();
            
            if (token == null
                || token.Status == TokenStatus.Invalid)
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", "Invalid token."));
            }

            return NoContent();
        }
        
        [HttpPost]
        [Route("[action]")]
        public IActionResult Invalidate([FromBody] TokenModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(403);
            }

            var tokens = from t in Db.Tokens
                where t.AccessToken == model.AccessToken
                      && (string.IsNullOrWhiteSpace(model.ClientToken) || model.ClientToken == t.ClientToken)
                      && t.Status != TokenStatus.Invalid
                select t;

            foreach (var token in tokens)
            {
                token.Status = TokenStatus.Invalid;
            }

            Db.SaveChanges();

            return NoContent();
        }
        
        [HttpPost]
        [Route("[action]")]
        public IActionResult Signout([FromBody] NamePassModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(403);
            }

            #region Check cooldown and password.

            var isEnableNonEmailLogin = Config.GetSection("Yggdrasil:Features:NonEmailLogin").Get<bool>();
            var isUsername = isEnableNonEmailLogin && !model.Username.Contains("@");
            
            var user = (from u in Db.Users
                where model.Username == (isUsername ? u.Username : u.Email)
                select u).FirstOrDefault();
            if (user == null)
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            var now = DateTime.Now;
            if (user.CoolDownEndTime > now)
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            user.TryTimes++;
            Db.SaveChanges();

            var maxTryTimes = Config.GetSection("Yggdrasil:Security:MaxTryTimes").Get<int>();
            if (user.TryTimes > maxTryTimes)
            {
                user.TryTimes = 0;
                user.CoolDownLevel++;
                user.CoolDownEndTime = now.AddMinutes(user.CoolDownLevel * user.CoolDownLevel * 5);
                Db.SaveChanges();
                
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            if (user.Password != Passwords.Hash(model.Password, user.PasswordSalt))
            {
                return StatusCode(403, 
                    new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid credentials. Invalid username or password."));
            }

            user.TryTimes = 0;
            user.CoolDownLevel = 0;
            user.CoolDownEndTime = now;
            Db.SaveChanges();
            
            #endregion

            #region Expire all tokens.

            foreach (var token in user.Tokens)
            {
                token.Status = TokenStatus.Invalid;
            }

            Db.SaveChanges();

            #endregion

            return NoContent();
        }
    }
}