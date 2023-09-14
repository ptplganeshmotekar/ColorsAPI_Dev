using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using Polly;
using PTPL.FitOS.DataContext;
using PTPL.FitOS.Services;
using Serilog;
//using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace PTPL.FitOS.ColoursAPI
{
    public class Startup
    {
        /// <summary>
        /// Create a object of swaggerUI layer
        /// </summary>

        #region Variable
        public IConfiguration Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }
        SwaggerUI.SwaggerUI _objSwaggerUI;
        #endregion

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            /// Created a object of swagger UI and given an title , name and version etc

            _objSwaggerUI = new SwaggerUI.SwaggerUI("FitOS Color API", "FitOS Color API",
                Path.Combine(AppContext.BaseDirectory, "ptplfitOS.Core.xml"), "v1");

            /// This lines of code is use for configure the logs for the API application.
            Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Map(
                evt => evt.Level,
                (level, wt) => wt.RollingFile(Configuration.GetSection("Logging")["LogPath"] + level + "-{Date}.log"))
            .CreateLogger();

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddAutoMapper(typeof(Startup));

            services.AddAuthentication("Bearer").AddJwtBearer(options =>
            {
                options.Audience = Configuration.GetSection("Cognito")["Audience"]; //"6rnuta1ri8fis56h3goifibqfr";
                options.Authority = Configuration.GetSection("Cognito")["Authority"]; //"https://cognito-idp.ap-south-1.amazonaws.com/ap-south-1_PsT4zVGvV";                
            });

            //services.AddHttpClient("PaletteAPIs", config =>
            //{
            //    config.BaseAddress = new Uri(Configuration["Services:Palette"]);
            //}).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));
            services.AddHttpClient("DocumentAPIs", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Document"]);
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

            services.AddHttpClient("MaterialAPIs", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Material"]);
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

            services.AddHttpClient("SupplierAPIs", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Supplier"]);
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

            services.AddHttpClient("StyleAPIs", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Style"]);
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

            services.AddHttpClient("PermissionAPIs", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Permission"]);
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

            services.AddHttpClient("CoreAPIs", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Core"]);
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

            services.AddCors(o => o.AddPolicy("FitOSCorsPolicy", builder => builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest).AddJsonOptions(options => { options.JsonSerializerOptions.IgnoreNullValues = true; });
            services
           .AddControllers()
           .AddJsonOptions(options =>
           {
               options.JsonSerializerOptions.IgnoreNullValues = true;
           }).AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
           //.AddNewtonsoftJson(options =>
           //{
           //    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
           //});
            services.AddApiVersioning(option =>
            {
                option.ReportApiVersions = true;
                option.DefaultApiVersion = new ApiVersion(1, 0);
                option.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddControllers();

            //Configuring Redis Cache
            //var redis = ConnectionMultiplexer.Connect(Configuration.GetSection("redis")["redis.connection"]);
            //services.AddScoped(s => redis.GetDatabase());

            services.Configure<ConnectionString>(Configuration.GetSection("ConnectionStrings"));

            services.AddDbContextPool<GetDataDBContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContextPool<InsertUpdateDBContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection1")));

            services.AddTransient(_ => new GetDataDBContext(Configuration.GetConnectionString("DefaultConnection")));

            _objSwaggerUI.ConfigureServices(services);
            services.AddResponseCompression();

            var builder = new ContainerBuilder();
            builder.Populate(services);            

            //Register services that you want to invoke.

            builder.RegisterType<ColourServices>().As<IColourServices>();
            builder.RegisterType<ColourDocumentService>().As<IColourDocumentService>();
            builder.RegisterType<SequenceService>().As<ISequenceService>();
            builder.RegisterType<DocumentProvider>().As<IDocumentInterface>();
            builder.RegisterType<FileProvider>().As<IFileInterface>();
            builder.RegisterType<MaterialProvider>().As<IMaterialInterface>();            
            builder.RegisterType<SupplierProvider>().As<ISupplierInterface>();
            builder.RegisterType<StyleProvider>().As<IStyleInterface>();
            builder.RegisterType<PermissionProvider>().As<IPermissionInterface>();            
            builder.RegisterType<CoreProvider>().As<ICoreInterface>();
            builder.RegisterType<Encrypt_DecryptService>().As<IEncrypt_DecryptService>();
            
            services.AddMvc().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddMvc().AddNewtonsoftJson(options => 
            options.SerializerSettings.Formatting = Formatting.Indented);
            //Swagger API  Code
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Fitos API",
                    Description = "A Fitos Core API",
                    Version = "v1"
                });

                // Configure the XML comments file path for the Swagger JSON and UI.
                string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile));
            });            

            this.ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
    
            loggerFactory.AddSerilog();
            _objSwaggerUI.Configure(app, "");
            app.UseResponseCompression();
            app.UseCors("FitOSCorsPolicy");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
