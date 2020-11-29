using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.DbModels;
using Midgard.Models.Api.Setup;

namespace Midgard.Controllers.Api
{
    [ApiController]
    [Route("Api/[controller]")]
    public class SetupController : ControllerBase
    {
        private IConfiguration Config { get; }
        private MidgardContext Db { get; }

        public SetupController(IConfiguration config, MidgardContext context)
        {
            Config = config;
            Db = context;
        }

        [HttpPost]
        // [Route("[action]")]
        public IActionResult Initialize([FromBody] InitializeModel model)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        // [Route("[action]")]
        public IActionResult IsInitialized()
        {
            throw new NotImplementedException();
        }
    }
}