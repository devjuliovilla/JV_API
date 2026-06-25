using Mapster;
using Microsoft.EntityFrameworkCore;
using MediatR;
using JV_API.Infrastructure.Persistence;
using JV_API.Shared.DTOs.Products;
using JV_API.Shared.Exceptions;

namespace JV_API.API.Endpoints.Products.Get;

public class GetProductHandler(AppDbContext db) : IRequestHandler<GetProductQuery, ProductResponse>
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
