using MediatR;
using JV_API.Shared.DTOs;
using JV_API.Shared.DTOs.Products;

namespace JV_API.API.Endpoints.Products.List;

public record GetProductsQuery : IRequest<PagedResponseDto<ProductResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Filters { get; init; }
    public string? Sorts { get; init; }
}
