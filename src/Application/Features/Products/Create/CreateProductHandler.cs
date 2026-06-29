using Application.Abstractions.Persistence;
using Mapster;
using MediatR;
using Domain.Entities;
using Application.Features.Products.Create;

namespace Application.Features.Products.Create;

public class CreateProductHandler(IAppDbContext db) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = request.Adapt<Product>();
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return product.Adapt<CreateProductResponse>();
    }
}
