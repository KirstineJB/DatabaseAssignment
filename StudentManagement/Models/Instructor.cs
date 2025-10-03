using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    public class Instructor
    {
        public int Id { get; set; } // PK
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }

        // Navigation
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
