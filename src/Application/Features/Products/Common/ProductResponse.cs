namespace Application.Features.Products.Common;

public class ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
