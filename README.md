Employee Management System (EMS) - Backend API
A robust, enterprise-grade RESTful API built with .NET 8.0 and Entity Framework Core for managing employee records, bank details, documents, department hierarchies, leave applications, shifts, attendance tracking, and payroll processing.

??? Technology Stack & Architecture
Framework: .NET 8.0 Web API
ORM: Entity Framework Core
Database: Microsoft SQL Server (with Stored Procedures for core transaction-heavy database operations)
Logging: Serilog (structured file-based logging)
Mapping: AutoMapper
Validation: FluentValidation
Export Formats: ClosedXML (Excel Reports)
Architecture Layout
Consolidated single-project design containing distinct logical layers:

Controllers: REST endpoints handling HTTP requests and routing.
Application: Core business services, interfaces, DTO definitions, FluentValidation rules, and AutoMapper profiles.
Domain: Pure business entities, enums, and database stored procedure name constants.
Infrastructure: Persistence layers (ApplicationDbContext), repository implementations, physical file storage handlers, and Excel export services.
?? Getting Started
Prerequisites
.NET 8.0 SDK
Microsoft SQL Server
Database Configuration
Update the database connection string in appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=SSManagementDEV;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;"
}

Run the Application
Restore packages and run the API:

dotnet restore
dotnet run

By default, the API will start and listen on port 6000 (or as configured in Properties/launchSettings.json).

?? Features
Employee Management: Complete basic profiles, upload Aadhaar card/bank passbook documents, and track active statuses.
Attendance & Shift Management: Assign employees to morning/night shifts on the machine floor and track check-in/check-out times (configured to local IST).
Leave Management: Standardized flow for applying for leaves and changing leave statuses.
Payroll Processing: Monthly payroll generation with incentive/allowance/advance adjustments, payslip downloading, and report exports.
Excel Exporter: Custom Excel exports designed with clean, cell-width adjusted, professional color palettes.
