using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Midgard.Utilities;
using NLog;
using NLog.Web;

namespace Midgard
{
    public class Program
    {
        #region Global

        public static readonly string Server = "Midgard";
        public static readonly string Author = "AmemiyaShigure";
        public static readonly Version Version = new(1, 0, 0, 0);

        #endregion

        #region Data

        internal static RSA RsaKey { get; set; }
        internal static string RsaPublicKey { get; set; }

        #endregion
        
        public static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            if (RsaKey == null)
            {
                logger.Info($"Starting Midgard, Server version is {Version}.");
                logger.Info($"Powered by {Author}.");
                logger.Info("Loading RSA key pair. Please wait...");
                Signature.Load();
            }

            var dir = Directory.GetCurrentDirectory();
            var skinsDir = Path.Combine(dir, "wwwroot", "skins");
            var skinsTempDir = Path.Combine(dir, "wwwroot", "skins", "temp");
            if (!Directory.Exists(skinsDir))
            {
                Directory.CreateDirectory(skinsDir);
            }
            if (!Directory.Exists(skinsTempDir))
            {
                Directory.CreateDirectory(skinsTempDir);
            }

            
            CreateHostBuilder(args).Build().Run();

            LogManager.Shutdown();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>()
                    .UseKestrel(kestrel =>
                    {
                        var config = (IConfiguration)kestrel.ApplicationServices.GetService(typeof(IConfiguration));

                        foreach (var listener in config.GetSection("Listeners").GetChildren())
                        {
                            if (bool.TryParse(listener["Enable"], out var isEnabled))
                            {
                                if (isEnabled)
                                {
                                    if (int.TryParse(listener["Port"], out var port))
                                    {
                                        kestrel.Listen(IPAddress.Any, port, option =>
                                        {
                                            option.UseConnectionLogging();
                                            if (!string.IsNullOrWhiteSpace(listener["Cert"] + listener["Password"]))
                                            {
                                                option.UseHttps(listener["Cert"], listener["Password"]);
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                    }).UseNLog(); });
    }
}