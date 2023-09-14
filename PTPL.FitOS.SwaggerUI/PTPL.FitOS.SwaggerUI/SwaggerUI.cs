using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PTPL.FitOS.SwaggerUI
{
    /// <summary>
    /// This class and implementation is use to set SwaggerUI for the library module.
    /// </summary>
    /// <remarks>Code documentation and Review is pending</remarks>
    public class SwaggerUI
    {
        /// <summary>
        /// This section is to declare local variables
        /// </summary>
        /// <remarks>Code documentation and Review is pending</remarks>
        public string Title { get; set; }     
        public string Version { get; set; }
        public string XMLPath { get; set; }
        public string Description { get; set; }
        public SwaggerUI(string title, string description, string xmlPath, string version)
        {
            XMLPath = xmlPath;
            Title = title;
            Version = version;
            Description = description;
        }

        /// <summary>
        /// This void method is use to set configure service for swagger UI
        /// </summary>
        /// <param name="services"></param>
        /// <remarks>Code documentation and Review is pending</remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(Version, new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = Version,
                    Title = Title,
                    Description = Description
                });

                // Set the comments path for the Swagger JSON and UI.               
                //c.IncludeXmlComments(XMLPath);
            });
        }

        /// <summary>
        /// This method is use to set configuration for the Swagger UI
        /// </summary>
        /// <param name="app"></param>
        /// <param name="jsonPath"></param>
        public void Configure(IApplicationBuilder app, string jsonPath)
        {
            if (jsonPath == "")
                jsonPath = "/swagger/v1/swagger.json";
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "v1.0");
                c.EnableFilter();
                c.DisplayRequestDuration();
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                c.DefaultModelExpandDepth(3);
            });

        }
    }
}
