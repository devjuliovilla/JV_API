using MediatR;
using Application.Common;
using Application.Features.Products.Common;

namespace Application.Features.Products.List;

public class GetProductsQuery : PagedRequestDto, 
    IRequest<PagedResponseDto<ProductResponse>>
{
}
