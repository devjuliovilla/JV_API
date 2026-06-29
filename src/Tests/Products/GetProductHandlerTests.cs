using Application.Exceptions;
using Application.Features.Products.Get;
using Microsoft.EntityFrameworkCore;

namespace Tests.Products;

public class GetProductHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var product = await context.Products.FirstAsync();
        var handler = new GetProductHandler(context);

        // Act
        var response = await handler.Handle(new GetProductQuery(product.Id), CancellationToken.None);

        // Assert
        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.False(string.IsNullOrWhiteSpace(response.CategoryName));
    }

    [Fact]
    public async Task Handle_WithMissingProduct_ThrowsNotFoundException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new GetProductHandler(context);

        // Act
        var act = () => handler.Handle(new GetProductQuery(999), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
