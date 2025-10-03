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
        public int Credits { get; set; }

     
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
