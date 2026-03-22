using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Review
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int ProductId { get; set; }

    public int ReviewerId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual OrderTable? Order { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
