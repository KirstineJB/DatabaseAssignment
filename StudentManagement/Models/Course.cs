using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    public class Course
    {
        public int Id { get; set; } // PK
        public string Title { get; set; } = string.Empty;
        public decimal Credits { get; set; }  //Ændret

        public int? InstructorId { get; set; } //Added
        public Instructor? Instructor { get; set; } //Added
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
