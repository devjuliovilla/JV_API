using Mapster;
using MediatR;
using JV_API.Domain.Entities;
using JV_API.Infrastructure.Persistence;
using JV_API.Shared.DTOs.Products;

namespace JV_API.API.Endpoints.Products.Create;

public class CreateProductHandler(AppDbContext db) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = request.Adapt<Product>();
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return product.Adapt<CreateProductResponse>();
    }
}
