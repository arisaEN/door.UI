using System;
using System.Collections.Generic;

namespace door.Domain.Entities;

public partial class DataEntry
{
    public int Id { get; set; }

    public string Date { get; set; } = null!;

    public string Time { get; set; } = null!;

    public string Status { get; set; } = null!;
}
