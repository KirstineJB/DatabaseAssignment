using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement
{
    public class SchoolContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Department> Departments { get; set; }
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<Department>(e =>
            {
                e.Property(d => d.Name).IsRequired().HasMaxLength(100);
                e.Property(d => d.Budget).HasColumnType("decimal(18,2)");
                e.Property(d => d.StartDate).HasColumnType("date");

                e.HasOne(d => d.DepartmentHead)
                 .WithMany(i => i.DepartmentsHeaded)    
                 .HasForeignKey(d => d.DepartmentHeadId)
                 .OnDelete(DeleteBehavior.SetNull);      // behold department hvis head of department bliver slettet

                
                e.HasIndex(d => d.DepartmentHeadId);
            });

        }
    }
}
