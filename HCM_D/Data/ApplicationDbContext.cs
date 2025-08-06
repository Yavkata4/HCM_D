using HCM_D.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SalaryHistory> SalaryHistories { get; set; }
        public DbSet<DepartmentGrowth> DepartmentGrowths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Department-Employee relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SalaryHistory-Employee relationship
            modelBuilder.Entity<SalaryHistory>()
                .HasOne<Employee>()
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DepartmentGrowth-Department relationship
            modelBuilder.Entity<DepartmentGrowth>()
                .HasOne(dg => dg.Department)
                .WithMany()
                .HasForeignKey(dg => dg.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure unique constraint for DepartmentGrowth (one record per department per year)
            modelBuilder.Entity<DepartmentGrowth>()
                .HasIndex(dg => new { dg.DepartmentId, dg.Year })
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryHistory>()
                .Property(s => s.OldSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryHistory>()
                .Property(s => s.NewSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DepartmentGrowth>()
                .Property(dg => dg.Revenue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DepartmentGrowth>()
                .Property(dg => dg.Expenses)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DepartmentGrowth>()
                .Property(dg => dg.DepartmentGoal)
                .HasPrecision(18, 2);
        }
    }
}
