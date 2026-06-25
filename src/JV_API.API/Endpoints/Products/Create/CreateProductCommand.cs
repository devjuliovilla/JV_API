using MediatR;
using JV_API.Shared.DTOs.Products;

namespace JV_API.API.Endpoints.Products.Create;

public record CreateProductCommand(string Name, string? Description, decimal Price, long CategoryId) : IRequest<CreateProductResponse>;
