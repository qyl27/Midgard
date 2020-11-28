using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.DbModels;
using Midgard.SharedModels;
using Midgard.Utilities;

namespace Midgard.Controllers.Yggdrasil
{
    [ApiController]
    [Route("Yggdrasil/Api/Profiles")]
    public class ApiController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }
        
        public ApiController(IConfiguration config, MidgardContext context)
        {
            Config = config;
            Db = context;
        }

        [HttpPost]
        [Route("Minecraft")]
        public IActionResult Query([FromBody] List<string> usernames)
        {
            var profiles = Db.Profiles.Where(u => usernames.Contains(u.Name));
            List<ProfileInformationModel> result = new();
            foreach (var profile in profiles)
            {
                result.Add(InformationSerializer.ProfileSerializer(profile));
            }

            return new JsonResult(result);
        }
    }
}