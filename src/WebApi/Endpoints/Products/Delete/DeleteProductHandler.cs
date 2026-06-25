using Microsoft.EntityFrameworkCore;
using MediatR;
using Infrastructure.Persistence;
using Shared.Exceptions;

namespace WebApi.Endpoints.Products.Delete;

public class DeleteProductHandler(AppDbContext db) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Product with id {request.Id} not found");

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
