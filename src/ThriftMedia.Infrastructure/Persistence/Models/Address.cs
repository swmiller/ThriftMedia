using System.ComponentModel.DataAnnotations;

namespace ThriftMedia.Data.Models;

public class Address
{
    [MaxLength(100)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(50)]
    public string City { get; set; } = string.Empty;

    [MaxLength(2)]
    public string State { get; set; } = string.Empty;

    [MaxLength(10)]
    public string ZipCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Country { get; set; } = "USA";
}