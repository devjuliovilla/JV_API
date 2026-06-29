using MediatR;

namespace Application.Features.Products.Delete;

public record DeleteProductCommand(long Id) : IRequest<Unit>;
