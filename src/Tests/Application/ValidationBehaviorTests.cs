using Application.Behaviors;
using Application.Features.Products.Create;
using FluentValidation;
using MediatR;

namespace Tests.Application;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var validators = new IValidator<CreateProductCommand>[] { new CreateProductValidator() };
        var behavior = new ValidationBehavior<CreateProductCommand, CreateProductResponse>(validators);
        var command = new CreateProductCommand(string.Empty, null, 0, 0);

        // Act
        var act = () => behavior.Handle(command, _ => Task.FromResult(new CreateProductResponse()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<global::Application.Exceptions.ValidationException>(() =>
            act());
    }

    [Fact]
    public async Task Handle_WithValidCommand_CallsNext()
    {
        // Arrange
        var validators = new IValidator<CreateProductCommand>[] { new CreateProductValidator() };
        var behavior = new ValidationBehavior<CreateProductCommand, CreateProductResponse>(validators);
        var command = new CreateProductCommand("Product", null, 10, 1);
        var called = false;

        // Act
        var result = await behavior.Handle(command, _ =>
        {
            called = true;
            return Task.FromResult(new CreateProductResponse { Id = 1, Name = command.Name });
        }, CancellationToken.None);

        // Assert
        Assert.True(called);
        Assert.Equal("Product", result.Name);
    }
}
