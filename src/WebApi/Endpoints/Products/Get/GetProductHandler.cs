using Mapster;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Infrastructure.Persistence;
using Shared.DTOs.Products;
using Shared.Exceptions;

namespace WebApi.Endpoints.Products.Get;

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
