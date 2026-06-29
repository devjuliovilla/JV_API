using Application.Abstractions.Persistence;
using Application.Exceptions;
using Application.Features.Products.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Products.Get;

public class GetProductHandler(IAppDbContext db) : IRequestHandler<GetProductQuery, ProductResponse>
{
    public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Product with id {request.Id} not found");

        return product;
    }
}
