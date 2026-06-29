using Mapster;
using Xunit;
using Application.Features.Products.Create;
using Domain.Entities;

namespace Tests.Products;

public class CreateProductTests : TestBase
{
    [Fact]
    public async Task CreateProduct_SavesToDatabase()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);

        var category = await context.Categories.FindAsync(1L);
        Assert.NotNull(category);

        var product = new Product
        {
            Name = "New Product",
            Description = "Test product",
            Price = 50.00m,
            CategoryId = category.Id
        };

        // Act
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert
        var saved = await context.Products.FindAsync(product.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Product", saved.Name);
        Assert.Equal(50.00m, saved.Price);
    }

    [Fact]
    public async Task CreateProduct_MapToResponse()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test",
            Price = 100m
        };

        // Act
        var response = product.Adapt<CreateProductResponse>();

        // Assert
        Assert.Equal(1, response.Id);
        Assert.Equal("Test", response.Name);
    }
}
