using MediatR;
using JV_API.Shared.DTOs.Products;

namespace JV_API.API.Endpoints.Products.Get;

public record GetProductQuery(long Id) : IRequest<ProductResponse>;
