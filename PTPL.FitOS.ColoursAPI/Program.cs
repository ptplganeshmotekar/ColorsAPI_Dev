using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using PTPL.FitOS.Services;
using Polly;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using PTPL.FitOS.DataContext;
using PTPL.FitOS;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Autofac.Core;
using AutoMapper.Configuration;
using NLog;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/Nlog.config"));


builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    //builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
    //{
    //    options.Audience = builder.Configuration.GetSection("Cognito")["Audience"]; //"6rnuta1ri8fis56h3goifibqfr";
    //    options.Authority = builder.Configuration.GetSection("Cognito")["Authority"]; //"https://cognito-idp.ap-south-1.amazonaws.com/ap-south-1_PsT4zVGvV";
    //});

    builder.Services.AddHttpClient("DocumentAPIs", config =>
    {
        config.BaseAddress = new Uri(builder.Configuration["Services:Document"]);
    }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

    builder.Services.AddHttpClient("MaterialAPIs", config =>
    {
        config.BaseAddress = new Uri(builder.Configuration["Services:Material"]);
    }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

    builder.Services.AddHttpClient("SupplierAPIs", config =>
    {
        config.BaseAddress = new Uri(builder.Configuration["Services:Supplier"]);
    }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

    builder.Services.AddHttpClient("StyleAPIs", config =>
    {
        config.BaseAddress = new Uri(builder.Configuration["Services:Style"]);
    }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

    builder.Services.AddHttpClient("PermissionAPIs", config =>
    {
        config.BaseAddress = new Uri(builder.Configuration["Services:Permission"]);
    }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

    builder.Services.AddHttpClient("CoreAPIs", config =>
    {
        config.BaseAddress = new Uri(builder.Configuration["Services:Core"]);
    }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(500)));

    builder.Services.AddCors(o => o.AddPolicy("FitOSCorsPolicy", builder => builder.AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader()));
    builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest).AddJsonOptions(options => { options.JsonSerializerOptions.IgnoreNullValues = true; });
    builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
       options.JsonSerializerOptions.IgnoreNullValues = true;
    }).AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
    //.AddNewtonsoftJson(options =>
    //{
    //    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    //});
    builder.Services.AddApiVersioning(option =>
    {
        option.ReportApiVersions = true;
        option.DefaultApiVersion = new ApiVersion(1, 0);
        option.AssumeDefaultVersionWhenUnspecified = true;
    });

    builder.Services.AddControllers();

builder.Services.Configure<ConnectionString>(builder.Configuration.GetSection("ConnectionStrings"));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString1 = builder.Configuration.GetConnectionString("DefaultConnection1");
CommonDeclarations.ConnectionString = connectionString;
CommonDeclarations.ConnectionString1 = connectionString1;
//CommonDeclarations.UseMongoDB = builder.Configuration.GetValue<bool>("UseMongoDB");
builder.Services.AddDbContext<InsertUpdateDBContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString1));
});


//builder.Services.AddDbContext<GetDataDBContext>(options =>
//{
//    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString1));
//});
builder.Services.AddDbContextPool<GetDataDBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddTransient(_ => new GetDataDBContext(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("thisissecuritykeyandshouldBelong343")),
        ValidateAudience = true,
        ValidIssuer = "AppClaim",
        ValidAudience = "AppClaim",
        RequireExpirationTime = true
    };
});

builder.Services.AddScoped<IColourServices, ColourServices>();
builder.Services.AddScoped<IColourDocumentService, ColourDocumentService>();
builder.Services.AddScoped<ISupplierInterface, SupplierProvider>();
builder.Services.AddScoped<ISequenceService, SequenceService>();
builder.Services.AddScoped<IDocumentInterface, DocumentProvider>();
builder.Services.AddScoped<IEncrypt_DecryptService, Encrypt_DecryptService>();
builder.Services.AddScoped<IFileInterface, FileProvider>();
builder.Services.AddScoped<IMaterialInterface, MaterialProvider>();
builder.Services.AddScoped<ISupplierInterface, SupplierProvider>();
builder.Services.AddScoped<IStyleInterface, StyleProvider>();
builder.Services.AddScoped<IPermissionInterface, PermissionProvider>();
builder.Services.AddScoped<ICoreInterface, CoreProvider>();
builder.Services.AddScoped<IEncrypt_DecryptService, Encrypt_DecryptService>();

//Register services that you want to invoke.           
builder.Services.AddMvc().AddNewtonsoftJson(options =>
options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
builder.Services.AddMvc().AddNewtonsoftJson(options =>
options.SerializerSettings.Formatting = Formatting.Indented);
//builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseResponseCompression();
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

app.MapControllers();

app.Run();
