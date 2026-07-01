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
        IQueryable<ProductResponse> query = db.Products
            .Include(p => p.Category)
            .ProjectToType<ProductResponse>()
            .AsNoTracking();

        var result = await sieveProcessor.Apply(request, query).ToListAsync(cancellationToken);
        var totalCount = await sieveProcessor.Apply(request, query, applyPagination: false).CountAsync(cancellationToken);

        return new PagedResponseDto<ProductResponse>
        {
            Items = result,
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            TotalCount = totalCount
        };
    }
}
