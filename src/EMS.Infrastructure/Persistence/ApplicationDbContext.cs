using EMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<EmsEmployee> Employees { get; set; }
        public DbSet<EmsEmployeeBankDetail> BankDetails { get; set; }
        public DbSet<EmsEmployeeDocument> Documents { get; set; }
        public DbSet<EmsShift> Shifts { get; set; }
        public DbSet<EmsAttendance> Attendances { get; set; }
        public DbSet<EmsPayroll> Payrolls { get; set; }
        public DbSet<EmsSetting> Settings { get; set; }
        public DbSet<EmsDepartment> Departments { get; set; }
        public DbSet<EmsLeave> Leaves { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade deletes or constraints
            modelBuilder.Entity<EmsEmployee>()
                .HasOne(e => e.BankDetail)
                .WithOne(b => b.Employee)
                .HasForeignKey<EmsEmployeeBankDetail>(b => b.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmsEmployee>()
                .HasMany(e => e.Documents)
                .WithOne(d => d.Employee)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmsEmployee>()
                .HasMany(e => e.Shifts)
                .WithOne(s => s.Employee)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmsEmployee>()
                .HasMany(e => e.Attendances)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmsEmployee>()
                .HasMany(e => e.Payrolls)
                .WithOne(p => p.Employee)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmsEmployee>()
                .HasMany(e => e.Leaves)
                .WithOne(l => l.Employee)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
