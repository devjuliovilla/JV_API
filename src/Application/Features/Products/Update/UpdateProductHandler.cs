using Application.Abstractions.Persistence;
using Application.Exceptions;
using Application.Features.Products.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Products.Update;

public class UpdateProductHandler(IAppDbContext db) : IRequestHandler<UpdateProductCommand, ProductResponse>
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
