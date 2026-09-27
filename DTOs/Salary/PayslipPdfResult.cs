namespace EMS.API.DTOs.Salary;

public class PayslipPdfResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
}
