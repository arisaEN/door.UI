using System;
using System.Collections.Generic;

namespace door.Domain.Entities;

public partial class MasterDoorStatus
{
    public int Id { get; set; }

    public string DoorStatusName { get; set; } = null!;

    public virtual ICollection<DataEntry> DataEntries { get; set; } = new List<DataEntry>();
}
