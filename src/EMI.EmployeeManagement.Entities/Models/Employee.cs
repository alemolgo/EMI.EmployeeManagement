using System;
using System.Collections.Generic;

namespace EMI.EmployeeManagement.Entities.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = null!;

    public int EmployeeCurrentPositionId { get; set; }

    public decimal EmployeeSalary { get; set; }

    public string EmployeePasswordHash { get; set; } = null!;

    public virtual Position EmployeeCurrentPosition { get; set; } = null!;

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    public virtual ICollection<PositionHistory> PositionHistories { get; set; }
    = new List<PositionHistory>();
}
