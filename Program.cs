using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using EMS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/hrms-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://200.141.4.172:4300")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForHRMSEmployeeManagementBackend2026!");

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
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HRMS Backend API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// DI - Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));

// DI - Repositories
builder.Services.AddScoped<EMS.API.Repositories.Interfaces.IEmployeeRepository, EMS.API.Repositories.Implementations.EmployeeRepository>();
builder.Services.AddScoped<EMS.API.Repositories.Interfaces.IEmployeeShiftRepository, EMS.API.Repositories.Implementations.EmployeeShiftRepository>();
// builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<EMS.API.Repositories.IAttendanceRepository, EMS.API.Repositories.AttendanceRepository>();
builder.Services.AddScoped<EMS.API.Repositories.Interfaces.ISalaryDetailsRepository, EMS.API.Repositories.Implementations.SalaryDetailsRepository>();
// builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
// builder.Services.AddScoped<IActivityRepository, ActivityRepository>();

// DI - Services
builder.Services.AddScoped<EMS.API.Services.Interfaces.IEmployeeService, EMS.API.Services.Implementations.EmployeeService>();
builder.Services.AddScoped<EMS.API.Services.Interfaces.IEmployeeShiftService, EMS.API.Services.Implementations.EmployeeShiftService>();
// builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<EMS.API.Services.IAttendanceService, EMS.API.Services.AttendanceService>();
builder.Services.Configure<EMS.API.Configuration.CompanyDetailsOptions>(
    builder.Configuration.GetSection(EMS.API.Configuration.CompanyDetailsOptions.SectionName));
builder.Services.AddScoped<EMS.API.Services.Interfaces.IPayslipPdfService, EMS.API.Services.Implementations.PayslipPdfService>();
builder.Services.AddScoped<EMS.API.Services.Interfaces.ISalaryDetailsService, EMS.API.Services.Implementations.SalaryDetailsService>();
// builder.Services.AddScoped<ISalaryService, SalaryService>();
// builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
// builder.Services.AddScoped<IActivityService, ActivityService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
