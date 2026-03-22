using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Store
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public string StoreName { get; set; } = null!;

    public string? Description { get; set; }

    public string? BannerImageUrl { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User Seller { get; set; } = null!;
}
