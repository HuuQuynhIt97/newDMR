using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Helpers.Extensions
{
    public static class IApplicationBuilderExtension
    {
        public static IApplicationBuilder AddSwaggerExtention(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital mixing room system version 2.1.0");
            });

            return app;
        }

        public static IApplicationBuilder AddCorsExtention(this IApplicationBuilder app)
        {
            app.UseCors("CorsPolicy");
            app.UseCors(x => x
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           //    .SetIsOriginAllowed(origin => true) // allow any origin
                           .AllowCredentials()); // allow credentials

            return app;
        }

        public static IApplicationBuilder UseSpaExtension(this IApplicationBuilder app)
        {
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = @"wwwroot/ClientApp";
                //if (env.IsDevelopment())
                //{
                //    spa.Options.SourcePath = @"../dmr-spa";
                //    spa.UseAngularCliServer(npmScript: "start");
                //}
            });

            return app;
        }
    }
}
