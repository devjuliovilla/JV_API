using MediatR;
using Application.Features.Products.Create;

namespace Application.Features.Products.Create;

public record CreateProductCommand(string Name, string? Description, decimal Price, long CategoryId) : IRequest<CreateProductResponse>;
