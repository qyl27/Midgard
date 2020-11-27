using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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