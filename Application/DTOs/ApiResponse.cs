using System;

namespace EMS.Application.DTOs
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Count { get; set; }

        public ApiResponse() { }

        public ApiResponse(T? data, string message = "", int? count = null)
        {
            Data = data;
            Message = message;
            Count = count;
        }
    }
}
