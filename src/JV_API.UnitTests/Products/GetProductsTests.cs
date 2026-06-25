using Microsoft.EntityFrameworkCore;
using Xunit;
using JV_API.Shared.DTOs.Products;
using Mapster;

namespace JV_API.UnitTests.Products;

public class GetProductsTests : TestBase
{
    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        using var context = CreateDbContext();
        await SeedData(context);

        var products = await context.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .ToListAsync();

        Assert.NotEmpty(products);
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetProducts_ShouldRespectSoftDelete()
    {
        using var context = CreateDbContext();
        await SeedData(context);

        var product = await context.Products.FirstAsync();
        product.Deleted = true;
        await context.SaveChangesAsync();

        var activeProducts = await context.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .ToListAsync();

        Assert.Single(activeProducts);
    }
}
