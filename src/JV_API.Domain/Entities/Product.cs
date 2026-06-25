using JV_API.Domain.Common;

namespace JV_API.Domain.Entities;

public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public long CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
