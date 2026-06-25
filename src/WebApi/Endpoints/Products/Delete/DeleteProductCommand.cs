using MediatR;

namespace WebApi.Endpoints.Products.Delete;

public record DeleteProductCommand(long Id) : IRequest<Unit>;
