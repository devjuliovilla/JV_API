using MediatR;

namespace JV_API.API.Endpoints.Products.Delete;

public record DeleteProductCommand(long Id) : IRequest<Unit>;
