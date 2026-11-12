using FluentValidation;
using Microsoft.Extensions.Options;

namespace TeamHub.Application.Common.Models;

/// <summary>Validator dùng chung cho query phân trang; PageSize tối đa lấy từ cấu hình Paging:MaxPageSize.</summary>
public abstract class PagedQueryValidator<T> : AbstractValidator<T> where T : IPagedQuery
{
    protected PagedQueryValidator(IOptions<PagingOptions> options)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, options.Value.MaxPageSize);
    }
}
