using MediatR;
using Application.Features.Products.Common;

namespace Application.Features.Products.Update;

public record UpdateProductCommand(long Id, string Name, string? Description, decimal Price, long CategoryId) : IRequest<ProductResponse>;
