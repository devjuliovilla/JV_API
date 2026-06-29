using Application.Features.Products.Create;
using Microsoft.EntityFrameworkCore;

namespace Tests.Products;

public class CreateProductHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesProduct()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var category = await context.Categories.FirstAsync();
        var handler = new CreateProductHandler(context);
        var command = new CreateProductCommand("New Product", "Created by test", 99m, category.Id);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == response.Id);
        Assert.NotNull(product);
        Assert.Equal(command.Name, response.Name);
        Assert.Equal(command.Price, product.Price);
        Assert.Equal(category.Id, product.CategoryId);
    }
}
