using Application.Features.Products.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Mapster;

namespace Tests.Products;

public class GetProductsTests : TestBase
{
    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);

        // Act
        var products = await context.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .ToListAsync();

        // Assert
        Assert.NotEmpty(products);
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetProducts_ShouldRespectSoftDelete()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);

        var product = await context.Products.FirstAsync();
        product.Deleted = true;
        await context.SaveChangesAsync();

        // Act
        var activeProducts = await context.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .ToListAsync();

        // Assert
        Assert.Single(activeProducts);
    }
}
