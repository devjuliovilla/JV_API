using Mapster;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Infrastructure.Persistence;
using Shared.DTOs.Products;
using Shared.Exceptions;

namespace WebApi.Endpoints.Products.Update;

public class UpdateProductHandler(AppDbContext db) : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Product with id {request.Id} not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;

        await db.SaveChangesAsync(cancellationToken);

        return product.Adapt<ProductResponse>();
    }
}
