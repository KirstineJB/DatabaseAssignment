using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    public class Enrollment
    {
        public int Id { get; set; } // PK
        public int StudentId { get; set; } // FK
        public int CourseId { get; set; }  // FK
        public string? Grade { get; set; }

        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
