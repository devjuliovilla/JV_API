using Application.Exceptions;
using Application.Features.Products.Update;
using Microsoft.EntityFrameworkCore;

namespace Tests.Products;

public class UpdateProductHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithExistingProduct_UpdatesProduct()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var product = await context.Products.FirstAsync();
        var category = await context.Categories.FirstAsync();
        var handler = new UpdateProductHandler(context);
        var command = new UpdateProductCommand(product.Id, "Updated", "Updated description", 150m, category.Id);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await context.Products.FindAsync(product.Id);
        Assert.NotNull(updated);
        Assert.Equal(command.Name, response.Name);
        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.Description, updated.Description);
        Assert.Equal(command.Price, updated.Price);
    }

    [Fact]
    public async Task Handle_WithMissingProduct_ThrowsNotFoundException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new UpdateProductHandler(context);
        var command = new UpdateProductCommand(999, "Missing", null, 10m, 1);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
