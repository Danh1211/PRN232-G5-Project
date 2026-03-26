using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Address
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Phone { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;

    public bool IsDefault { get; set; }

    public virtual ICollection<OrderTable> OrderTables { get; set; } = new List<OrderTable>();

    public virtual User User { get; set; } = null!;
}
