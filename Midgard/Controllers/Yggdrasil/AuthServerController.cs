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
            #region Check cooldown and password.

            var isEnableNonEmailLogin = Config.GetSection("Yggdrasil:Features:NonEmailLogin").Get<bool>();
            var isUsername = isEnableNonEmailLogin && !model.Username.Contains("@");
            
            var user = (from u in Db.Users
                where model.Username == (isUsername ? u.Username : u.Email)
                select u).FirstOrDefault();
            if (user == null)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
            }

            var now = DateTime.Now;
            if (user.CoolDownEndTime > now)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
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
                
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
            }

            if (user.Password != Passwords.Hash(model.Password, user.PasswordSalt))
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
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
                AccessToken = accessToken.ToString("N"),
                ClientToken = clientToken, 
                AvailableProfiles = new()
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
            #region Check tokens.

            var token = Db.Tokens
                .FirstOrDefault(t => t.AccessToken == model.AccessToken && t.Status != TokenStatus.Invalid);
            
            if (token == null)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid token.")) { StatusCode = 403 };
            }

            if (!string.IsNullOrWhiteSpace(model.ClientToken))
            {
                if (token.ClientToken != model.ClientToken)
                {
                    return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid token.")) { StatusCode = 403 };
                }
            }

            if (token.BindProfile != null && model.SelectedProfile != null)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Access token already has a profile assigned.")) { StatusCode = 403 };
            }

            token.Status = TokenStatus.Invalid;
            Db.SaveChanges();

            #endregion

            #region Delete invalid tokens.

            var invalidTokens = Db.Tokens.Where(t => t.Status == TokenStatus.Invalid);
            foreach (var t in invalidTokens)
            {
                Db.Tokens.Remove(t);
            }
            Db.SaveChanges();

            #endregion

            #region Make Response.
           
            var now = DateTime.Now;
            var accessToken = Guid.NewGuid();
            var clientToken = model.ClientToken ?? Guid.NewGuid().ToString("N");
            
            var result = new RefreshViewModel()
            {
                AccessToken = accessToken.ToString("N"),
                ClientToken = clientToken
            };

            if (model.RequestUser)
            {
                result.User = InformationSerializer.UserSerializer(token.BindUser);
            }

            Profile profile = null;
            if (model.SelectedProfile is not null)
            {
                result.SelectedProfile = model.SelectedProfile;

                var guidParseResult = Guid.TryParse(model.SelectedProfile.Id, out var profileId);
                if (!guidParseResult)
                {
                    return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid token.")) { StatusCode = 403 };
                }
                profile = (from p in Db.Profiles
                    where p.Id == profileId
                    select p).FirstOrDefault();
                if (profile is null)
                {
                    return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                        "Invalid token.")) { StatusCode = 403 };
                }

                profile.IsSelected = true;
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
            var token = (from t in Db.Tokens
                where t.AccessToken == model.AccessToken
                      && (string.IsNullOrWhiteSpace(model.ClientToken) || model.ClientToken == t.ClientToken)
                      && t.Status != TokenStatus.Invalid
                select t).FirstOrDefault();
            
            if (token == null
                || token.Status == TokenStatus.Invalid)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid token.")) { StatusCode = 403 };
            }

            return NoContent();
        }
        
        [HttpPost]
        [Route("[action]")]
        public IActionResult Invalidate([FromBody] TokenModel model)
        {
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
            #region Check cooldown and password.

            var isEnableNonEmailLogin = Config.GetSection("Yggdrasil:Features:NonEmailLogin").Get<bool>();
            var isUsername = isEnableNonEmailLogin && !model.Username.Contains("@");
            
            var user = (from u in Db.Users
                where model.Username == (isUsername ? u.Username : u.Email)
                select u).FirstOrDefault();
            if (user == null)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
            }

            var now = DateTime.Now;
            if (user.CoolDownEndTime > now)
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
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
                
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
            }

            if (user.Password != Passwords.Hash(model.Password, user.PasswordSalt))
            {
                return new JsonResult(new ErrorViewModel("ForbiddenOperationException", 
                    "Invalid credentials. Invalid username or password.")) { StatusCode = 403 };
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