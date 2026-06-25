using Mapster;
using MediatR;
using Domain.Entities;
using Infrastructure.Persistence;
using Shared.DTOs.Products;

namespace WebApi.Endpoints.Products.Create;

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
