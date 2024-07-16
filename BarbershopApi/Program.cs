using BarbershopApi;
using BarbershopBL.Interfaces;
using BarbershopBL.Services;
using BarbershopBL.Validators;
using BarbershopDL;
using BarbershopDL.EF.Contexts;
using BarbershopDL.Interfaces;
using BarbershopDL.Services;
using BarbershopEntities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication
    .CreateBuilder(args);

builder.UseSerilog();
builder.Services.Configure<AppSettings>(builder.Configuration);

AppSettings appSettings = builder.Configuration.Get<AppSettings>();

// Add services to the container.
builder.Services.AddScoped<IAppointmentBL, AppointmentBL>();
builder.Services.AddScoped<IAppointmentDL, AppointmentDL>();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserDL, UserDL>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(MapperManager));

// הוספת שירותי FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<UserLoginDTOValidator>())
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<UserToAddDTOValidator>())
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AppointmentToAddDTOValidator>());




builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = appSettings.Jwt.Issuer,
            ValidAudience = appSettings.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies[CookiesKeys.AccessToken];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        );
});

builder.Services.AddDbContext<BarbershopContext>(option => {
    option.UseSqlServer(appSettings.connectionStrings.Barbershop);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("CorsPolicy");


app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
