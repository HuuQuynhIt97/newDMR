using DMR_API.Data;
using DMR_API.Helpers;
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.CookiePolicy;
using DMR_API.Helpers.Extensions;
using dmr_api.Data;

namespace DMR_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appsettings = Configuration.GetSection("Appsettings").Get<Appsettings>();

            services.AddDatabaseExtention(Configuration)
                    .AddRepositoriesExtention()
                    .AddServicesExtention();

            services.AddRedisCacheExtention();

            services.AddSignalR();

            services.AddLogging();
          
            services.AddControllers()
                    .AddNewtonsoftJson(options =>
                                    {
                                        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                                    });

            services.AddCorsExtension(Configuration);

            services.AddTransient<DbInitializer>();
            //Auto Mapper
            services.AddAutoMapperExtention();

            services.AddAuthenticationWithSwaggerExtention(Configuration);

            services.AddHttpClientExtention(Configuration);

            services.AddShedulerExtention(Configuration);

            services.AddSpaExtention();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext dataContext)
        {

            dataContext.Database.Migrate();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.AddSwaggerExtention();
            app.AddCorsExtention();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
           
            app.UseAuthentication()
               .UseCookiePolicy(new CookiePolicyOptions
               {
                    HttpOnly = HttpOnlyPolicy.Always
               });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ECHub>("/ec-hub");

            });

            app.UseSpaExtension();

           
        }
    }
}
