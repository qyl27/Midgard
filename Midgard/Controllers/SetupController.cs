using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.Models.Setup;
using Midgard.ViewModels.Setup;

namespace Midgard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SetupController : ControllerBase
    {
        private IConfiguration Config { get; }

        public SetupController(IConfiguration config)
        {
            Config = config;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Initialize([FromBody] InitializeModel model)
        {
            
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult IsInitialized()
        {
            if (bool.TryParse(Config["General:Initialized"], out var isInitialized))
            {
                return new JsonResult(new InitializeViewModel
                {
                    Result = isInitialized
                });
            }
            
            return new JsonResult(new InitializeViewModel
            {
                Result = false
            });
        }
    }
}