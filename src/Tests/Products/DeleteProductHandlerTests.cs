using Application.Exceptions;
using Application.Features.Products.Delete;
using Microsoft.EntityFrameworkCore;

namespace Tests.Products;

public class DeleteProductHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithExistingProduct_RemovesProduct()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var product = await context.Products.FirstAsync();
        var handler = new DeleteProductHandler(context);

        // Act
        await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        Assert.Null(await context.Products.FindAsync(product.Id));
    }

    [Fact]
    public async Task Handle_WithMissingProduct_ThrowsNotFoundException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new DeleteProductHandler(context);

        // Act
        var act = () => handler.Handle(new DeleteProductCommand(999), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
