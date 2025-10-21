using System;
using System.Collections.Generic;

namespace ThriftMedia.Data.Models;

public partial class Store
{
    public int Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public Address Address { get; set; } = new Address();
    public string? PhoneNumber { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual ICollection<Media> MediaItems { get; set; } = new List<Media>();
}