using System;
using System.Collections.Generic;

namespace ThriftMedia.Data.Models;

public partial class Media
{
    public int MediaId { get; set; }
    public int StoreId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string OcrPayloadJson { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual Store Store { get; set; } = null!;
}