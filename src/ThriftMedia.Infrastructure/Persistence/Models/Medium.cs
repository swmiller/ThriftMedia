using System;
using System.Collections.Generic;

namespace ThriftMedia.Infrastructure.Persistence.Models;

public partial class Medium
{
    public int MediaId { get; set; }

    public string MediaType { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string OcrPayloadJson { get; set; } = null!;

    public bool? IsTested { get; set; }

    public decimal? Price { get; set; }

    public string? ShelfLocation { get; set; }

    public string? Condition { get; set; }

    public int StoreId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;
}
