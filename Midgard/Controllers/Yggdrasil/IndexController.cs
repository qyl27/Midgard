using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Midgard.ViewModels.Yggdrasil.Index;

namespace Midgard.Controllers.Yggdrasil
{
    [ApiController]
    [Route("Yggdrasil")]
    [Route("Yggdrasil/[controller]")]
    public class IndexController : ControllerBase
    {
        private IConfiguration Config { get; }
        
        public IndexController(IConfiguration config)
        {
            Config = config;
        }
        
        [HttpGet]
        [Route("")]
        [Route("[action]")]
        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
            {
                SkinDomains = Config.GetSection("Yggdrasil:SkinDomains").Get<List<string>>(),
                SignaturePublickey = Program.RsaPublicKey,
                Meta = new MetaViewModel
                {
                    ImplementationName = Program.Server, 
                    ImplementationAuthor = Program.Author, 
                    ImplementationVersion = Program.Version.ToString(), 
                    ServerName = Config["Yggdrasil:ServerName"], 
                    ServerOwner = Config["Yggdrasil:Owner"], 
                    ServerOwnerContact = Config["Yggdrasil:Contact"], 
                    Links = new LinksViewModel
                    {
                        Homepage = Config["Yggdrasil:Pages:Homepage"], 
                        Register = Config["Yggdrasil:Pages:Register"], 
                        Manage = Config["Yggdrasil:Pages:Manage"]
                    }, 
                    FeatureNonEmailLogin = Config.GetSection("Yggdrasil:Features:NonEmailLogin").Get<bool>()
                } 
            };

            return new JsonResult(viewModel);
        }
    }
}