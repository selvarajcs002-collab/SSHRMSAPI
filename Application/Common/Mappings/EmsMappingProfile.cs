using AutoMapper;
using EMS.Application.DTOs;
using EMS.Domain.Entities;

namespace EMS.Application.Common.Mappings
{
    public class EmsMappingProfile : Profile
    {
        public EmsMappingProfile()
        {
            // Employee mappings
            CreateMap<EmployeeBasicDetailCreateDto, EmsEmployee>()
                .ForMember(dest => dest.PerDaySalary, opt => opt.MapFrom(src => src.SalaryPerMonth));
            CreateMap<EmsEmployee, EmployeeBasicDetailDto>()
                .ForMember(dest => dest.SalaryPerMonth, opt => opt.MapFrom(src => src.PerDaySalary));
            CreateMap<EmployeeBankDetailCreateDto, EmsEmployeeBankDetail>();
            CreateMap<EmsEmployeeBankDetail, EmployeeBankDetailDto>();
            CreateMap<EmsEmployeeDocument, EmployeeDocumentDto>();
            
            // Shift mappings
            CreateMap<EmsShift, ShiftAssignmentDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty));
            CreateMap<ShiftAssignmentCreateDto, EmsShift>();

            // Attendance mappings
            CreateMap<EmsAttendance, AttendanceDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
                .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Designation : string.Empty))
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.Status == EMS.Domain.Enums.EmsAttendanceStatus.Absent ? "-" : src.CreatedAt.ToString("hh:mm tt")))
                .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => src.Status == EMS.Domain.Enums.EmsAttendanceStatus.Absent ? "-" : (src.Remarks == "Half Day" ? src.CreatedAt.AddHours(4).ToString("hh:mm tt") : src.CreatedAt.AddHours(9).ToString("hh:mm tt"))));
            CreateMap<AttendanceMarkDto, EmsAttendance>();

            // Payroll mappings
            CreateMap<EmsPayroll, PayrollDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty));
            CreateMap<PayrollGenerateDto, EmsPayroll>();

            // Settings mappings
            CreateMap<EmsSetting, SettingDto>();

            // Department mappings
            CreateMap<EmsDepartment, DepartmentDto>();
            CreateMap<DepartmentCreateDto, EmsDepartment>();

            // Leave mappings
            CreateMap<EmsLeave, LeaveDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty));
            CreateMap<LeaveApplyDto, EmsLeave>();
        }
    }
}
