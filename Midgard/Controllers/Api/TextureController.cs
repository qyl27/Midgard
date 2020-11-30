using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Midgard.DbModels;
using Midgard.Enumerates;
using Midgard.Models.Api.Skin;
using Midgard.Utilities;
using Midgard.ViewModels.Api.Shared;

namespace Midgard.Controllers.Api
{
    [ApiController]
    [Route("Api/[controller]")]
    public class TextureController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        private IHostEnvironment Env { get; }

        public TextureController(IConfiguration config, MidgardContext context, IHostEnvironment environment)
        {
            Config = config;
            Db = context;
            Env = environment;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Skin([FromForm] AddSkinModel model)
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

            var rootPath = Env.ContentRootPath;
            var tmpName = Time.GetTimeStamp13(DateTime.Now).ToString();
            var fullTempPath = Path.Combine(rootPath, "wwwroot", "skins", "temp", tmpName);

            using var stream = new FileStream(fullTempPath, FileMode.CreateNew);
            model.File.CopyTo(stream);

            if (!Texture.Check(stream))
            {
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.bomb_detected"
                    }
                });
            }

            using var image = Image.FromStream(stream);
            if (image.Width % 64 != 0 || image.Height % 32 != 0)
            {
                stream.Dispose();
                System.IO.File.Delete(fullTempPath);
                return new JsonResult(new ErrorViewModel
                {
                    Message = new()
                    {
                        Message = "message.error.texture_illegal"
                    }
                });
            }

            using var bitmap = new Bitmap(image);
            var hash = Texture.Hash(bitmap);
            var savePath = Path.Combine(rootPath, "wwwroot", "skins", hash);
            bitmap.Save(savePath);

            bitmap.Dispose();
            image.Dispose();
            stream.Dispose();
            System.IO.File.Delete(fullTempPath);
            
            #endregion
            
            #region Do add skin.
            
            var saveDomain = Config.GetSection("General:SkinSaveDomain").Get<string>();
            

            user.Skins ??= new();
            user.Skins.Add(new Skin
            {
                Id = Guid.NewGuid(), 
                Name = model.Name, 
                Model = model.IsSlim ? SkinModel.Slim : SkinModel.Default, 
                Url = $"{saveDomain}/skins/{hash}"
            });
            Db.SaveChanges();

            return NoContent();

            #endregion
        }
    }
}