using MediatR;
using JV_API.Shared.DTOs.Products;

namespace JV_API.API.Endpoints.Products.Update;

public record UpdateProductCommand(long Id, string Name, string? Description, decimal Price, long CategoryId) : IRequest<ProductResponse>;
