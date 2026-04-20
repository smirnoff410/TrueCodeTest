using Common.Command;
using Common.Repository;
using Common.Settings;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Command;
using UserService.Application.Repository;
using UserService.Application.Request.User;
using UserService.Application.Services;
using UserService.Infrastracture.Persistence;
using UserService.Infrastracture.Repository;
using UserService.Infrastracture.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region
var con = builder.Configuration.GetConnectionString("MasterConnection");
builder.Services.AddDbContext<UserServiceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ICommandHandler<RegisterUserRequest, string>, RegisterUserCommand>();
builder.Services.AddScoped<ICommandHandler<LoginRequest, string>, LoginCommand>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
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
    var db = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
    db.Database.Migrate();
}

app.Run();
