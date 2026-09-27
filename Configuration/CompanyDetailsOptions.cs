namespace EMS.API.Configuration;

public class CompanyDetailsOptions
{
    public const string SectionName = "CompanyDetails";

    public string CompanyName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string GSTNumber { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
}
