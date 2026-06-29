using Application.Abstractions.Persistence;
using Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Products.Delete;

public class DeleteProductHandler(IAppDbContext db) : IRequestHandler<DeleteProductCommand, Unit>
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
