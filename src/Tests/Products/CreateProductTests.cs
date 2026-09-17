using Mapster;
using Xunit;
using Application.Features.Products.Create;
using Domain.Entities;

namespace Tests.Products;

public class CreateProductTests : TestBase
{
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
