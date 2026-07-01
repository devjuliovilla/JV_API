using Sieve.Attributes;

namespace Application.Features.Products.Common;

public class ProductResponse
{
    [Sieve(CanFilter = true, CanSort = true)]
    public long Id { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Price { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string CategoryName { get; set; } = string.Empty;
}
