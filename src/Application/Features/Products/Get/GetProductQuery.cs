using MediatR;
using Application.Features.Products.Common;

namespace Application.Features.Products.Get;

public record GetProductQuery(long Id) : IRequest<ProductResponse>;
