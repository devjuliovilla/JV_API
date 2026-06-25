using MediatR;
using Shared.DTOs.Products;

namespace WebApi.Endpoints.Products.Create;

public record CreateProductCommand(string Name, string? Description, decimal Price, long CategoryId) : IRequest<CreateProductResponse>;
