using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Midgard.DbModels;
using Midgard.Enumerates;
using Midgard.Models.Api.Skin;
using Midgard.ViewModels.Api.Shared;

namespace Midgard.Controllers.Api
{
    public class TextureController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        private IHostingEnvironment Env { get; }

        public TextureController(IConfiguration config, MidgardContext context, IHostingEnvironment environment)
        {
            Config = config;
            Db = context;
            Env = environment;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Skin([FromBody] AddSkinModel model)
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

            #region Save skin.

            var maxFileLength = Config.GetSection("General:Security:MaxFileLength").Get<long>();
            if (model.File.Length > maxFileLength)
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.texture_too_big"
                    }
                }) {StatusCode = 403};
            }

            var saveDomain = Config.GetSection("General:SkinSaveDomain").Get<string>();
            var rootPath = Env.ContentRootPath;
            var fullPath = $"{rootPath}/skins/";

            // Todo: Save textures.
            throw new NotImplementedException();

            #endregion
            
            #region Do add skin.

            user.Skins ??= new();
            user.Skins.Add(new Skin
            {
                Id = Guid.NewGuid(), 
                Name = model.Name, 
                Model = model.IsSlim ? SkinModel.Slim : SkinModel.Default
            });

            return NoContent();

            #endregion
        }
    }
}