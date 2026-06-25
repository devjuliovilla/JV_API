using MediatR;
using Shared.DTOs.Products;

namespace WebApi.Endpoints.Products.Get;

public record GetProductQuery(long Id) : IRequest<ProductResponse>;
