using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMI.EmployeeManagement.Entities.Dto
{
    public class EmployeeResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int CurrentPositionId { get; set; }
        public decimal Salary { get; set; }
        public decimal bonus { get; set; }
    }
}
