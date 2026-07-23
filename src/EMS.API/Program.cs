using System;
using System.Text;
using System.Text.Json.Serialization;
using EMS.Application.Common.Mappings;
using EMS.Application.Interfaces;
using EMS.Application.Services;
using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Repositories;
using EMS.Infrastructure.Services;
using EMS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using EMS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog Centralized Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ems-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register Background Services
builder.Services.AddHostedService<EMS.API.Jobs.PayrollArchivalService>();

// Register Dependency Injection (Repositories)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();

// Register Dependency Injection (Services)
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Register Background Services
builder.Services.AddHostedService<AttendanceArchivalService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(EmsMappingProfile));

// CORS policy for Angular frontend (allow dynamic ports)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "DefaultSuperSecretKeyThatIsAtLeast32BytesLong!");

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
        // Convert enums to strings in API JSON responses
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authorization
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EMS Clean API",
        Version = "v1",
        Description = "Employee Management System (EMS) Clean Architecture API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Register Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Enable Swagger globally
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "EMS API v1");
    options.RoutePrefix = "swagger";
});

// Ensure the upload folder exists inside wwwroot or base uploads
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseCors("AllowAngularApp");

// Serve uploaded documents statically (if requested)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-migrate and self-heal database and stored procedures
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // First ensure DB is created
        await dbContext.Database.EnsureCreatedAsync();

        // Ensure missing columns exist (since EnsureCreated doesn't run migrations)
        var addColumnsSql = @"
            IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'DateOfBirth' AND Object_ID = Object_ID(N'EMS_Employees'))
            BEGIN
                ALTER TABLE EMS_Employees ADD DateOfBirth DATETIME2 NULL, Gender NVARCHAR(20) NULL, Nationality NVARCHAR(50) NULL;
            END
        ";
        await dbContext.Database.ExecuteSqlRawAsync(addColumnsSql);

        Log.Information("Database successfully ensured.");

        // Read and execute db_scripts.sql to apply stored procedures
        var scriptPath = Path.Combine(app.Environment.ContentRootPath, "..", "..", "db_scripts.sql");
        if (File.Exists(scriptPath))
        {
            var scriptText = await File.ReadAllTextAsync(scriptPath);
            
            // Split script by GO
            var batches = scriptText.Split(new[] { "GO\r\n", "GO\n", "GO\r", "\nGO", "\rGO" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    // Filter out USE database statements since EF Core is already connected to EMS_Database
                    var cleanBatch = batch.Trim();
                    if (cleanBatch.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    await dbContext.Database.ExecuteSqlRawAsync(cleanBatch);
                }
            }
            Log.Information("Stored procedures and database schema successfully updated from db_scripts.sql.");
        }
        else
        {
            Log.Warning($"Could not find db_scripts.sql at path: {scriptPath}. Stored procedures may not be initialized.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while ensuring/creating the database and stored procedures.");
    }
}

Log.Information("EMS Clean API started successfully.");
app.Run();
