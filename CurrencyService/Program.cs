using Common.Command;
using Common.Repository;
using Common.Settings;
using CurrencyService.Application.Command;
using CurrencyService.Application.Query;
using CurrencyService.Application.Repository;
using CurrencyService.Application.Request;
using CurrencyService.Application.Services;
using CurrencyService.Domain.Models;
using CurrencyService.Infrastracture.Persistence;
using CurrencyService.Infrastracture.Repository;
using CurrencyService.Infrastracture.Services;
using CurrencyService.Infrastracture.Services.Currency;
using CurrencyService.Web.BackgroundServices;
using CurrencyService.Web.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Currency API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
   {
     new OpenApiSecurityScheme
     {
       Reference = new OpenApiReference
       {
         Type = ReferenceType.SecurityScheme,
         Id = "Bearer"
       }
      },
      new string[] { }
    }
  });
});

#region
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var con = builder.Configuration.GetConnectionString("MasterConnection");
builder.Services.AddDbContext<CurrencyServiceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IUserCurrencyRepository, UserCurrencyRepository>();

builder.Services.AddScoped<ICommandHandler<List<ApplyCurrencyRequest>, bool>, ApplyCurrencyCommand>();
builder.Services.AddScoped<ICommandHandler<GetUserCurrencyRequest, List<Currency>>, GetUserCurrencyQuery>();
builder.Services.AddScoped<ICommandHandler<AddUserCurrencyRequest, Guid>, AddUserCurrencyCommand>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ICurrencyGrabber, CurrencyGrabber>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.Configure<CurrencyGrabberSettings>(builder.Configuration.GetSection("CurrencyGrabber"));
builder.Services.Configure<CurrencyBackgroundSettings>(builder.Configuration.GetSection("CurrencyBackground"));
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var secretKey = Encoding.ASCII.GetBytes(jwtSettingsSection["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettingsSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettingsSection["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddHostedService<CurrencyBackgroundService>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CurrencyServiceContext>();
    db.Database.Migrate();
}

app.Run();
