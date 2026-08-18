using System;

namespace EMS.Application.DTOs
{
    public class ErrorResponse
    {
        public string Status { get; set; } = "Error";
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
}
