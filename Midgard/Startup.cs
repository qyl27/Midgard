using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Midgard.DbModels;
using Newtonsoft.Json;
using NLog;

namespace Midgard
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private ILogger Log { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log = LogManager.GetCurrentClassLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Forward config.
            services.Configure<ForwardedHeadersOptions>(forwarded =>
            {
                forwarded.ForwardedHeaders = ForwardedHeaders.All;
            });
            
            // Database config.
            services.AddDbContext<MidgardContext>(option =>
            {
                var connectionString = $"Server={Configuration["Database:IP"]};" +
                                       $"Port={Configuration["Database:Port"]};" +
                                       $"Uid={Configuration["Database:Username"]};" +
                                       $"Pwd={Configuration["Database:Password"]};" +
                                       $"DataBase={Configuration["Database:Name"]};";
                option.UseLazyLoadingProxies()
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            
            // Controller config.
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
            
            // Swagger.
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Midgard", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Midgard v1"));
            }
            
            app.UseForwardedHeaders();
            
            app.Use(next =>
            {
                return async context =>
                {
                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers["Server"] = Program.Server;
                        context.Response.Headers["Author"] = Program.Author;
                        context.Response.Headers["Version"] = Program.Version.ToString();

                        return Task.CompletedTask;
                    });

                    Log.Info($"Got a {context.Request.Protocol} {context.Request.Method} request " +
                             $"from {context.Request.Host} to {context.Request.Path} " +
                             $"with endpoint {context.Connection.RemoteIpAddress?.MapToIPv4()}:{context.Connection.RemotePort} " +
                             $"and ID {context.Connection.Id}.");

                    await next(context);
                };
            });

            if (bool.TryParse(Configuration["General:Initialized"], out var isInitialized))
            {
                if (isInitialized)
                {
                    using var dbContext = app.ApplicationServices.CreateScope()
                        .ServiceProvider.GetRequiredService<MidgardContext>();
                    dbContext.Database.EnsureCreated();
                }
            }
            
            app.UseStaticFiles();
            
            app.UseDefaultFiles();
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}