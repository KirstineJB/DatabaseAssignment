using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    public class Department
    {
        public int Id { get; set; }                 // PK
        public string Name { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }


        public int? DepartmentHeadId { get; set; }
        public Instructor? DepartmentHead { get; set; }
    }
}
