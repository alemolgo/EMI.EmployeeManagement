using System;
using System.Collections.Generic;

namespace EMI.EmployeeManagement.Entities.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionName { get; set; } = null!;

    public bool PositionIsManager { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<PositionHistory> PositionHistories { get; set; } = new List<PositionHistory>();
}
