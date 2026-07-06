using System;
using System.Collections.Generic;

namespace EMI.EmployeeManagement.Entities.Models;

public partial class EmployeeRole
{
    public int EmployeeRoleId { get; set; }

    public int EmployeeId { get; set; }

    public int RoleId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
