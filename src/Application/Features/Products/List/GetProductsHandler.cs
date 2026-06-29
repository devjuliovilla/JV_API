using Application.Abstractions.Persistence;
using Application.Common;
using Application.Features.Products.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Sieve.Services;

namespace Application.Features.Products.List;

public class GetProductsHandler(IAppDbContext db, ISieveProcessor sieveProcessor) : IRequestHandler<GetProductsQuery, PagedResponseDto<ProductResponse>>
{
    public async Task<PagedResponseDto<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .AsNoTracking();

        var sieveModel = new Sieve.Models.SieveModel
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Filters = request.Filters,
            Sorts = request.Sorts
        };

        var result = await sieveProcessor.Apply(sieveModel, query).ToListAsync(cancellationToken);
        var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken);

        return new PagedResponseDto<ProductResponse>
        {
            Items = result,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
