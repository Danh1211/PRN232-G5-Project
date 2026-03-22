using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Message
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime Date { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual Room Room { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
