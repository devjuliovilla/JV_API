using MediatR;
using Shared.DTOs.Products;

namespace WebApi.Endpoints.Products.Update;

public record UpdateProductCommand(long Id, string Name, string? Description, decimal Price, long CategoryId) : IRequest<ProductResponse>;
