using System;
using System.Collections.Generic;

namespace EMI.EmployeeManagement.Entities.Models;

public partial class PositionHistory
{
    public int PositionHistoryId { get; set; }

    public int PositionHistoryEmployeeId { get; set; }

    public int PositionHistoryPositionId { get; set; }

    public bool PositionHistoryIsActive { get; set; }

    public DateTime PositionHistoryStartDate { get; set; }

    public DateTime? PositionHistoryEndDate { get; set; }

    public virtual Employee PositionHistoryEmployee { get; set; } = null!;

    public virtual Position PositionHistoryPosition { get; set; } = null!;
}
