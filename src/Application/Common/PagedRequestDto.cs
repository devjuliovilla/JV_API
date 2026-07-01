using Sieve.Models;

namespace Application.Common;

public class PagedRequestDto : SieveModel
{
    public PagedRequestDto()
    {
        Page = 1;
        PageSize = 25;
    }
}
