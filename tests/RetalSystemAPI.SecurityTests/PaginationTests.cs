using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Services.Common.Models;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 0, 1, Pagination.DefaultPageSize)]
    [InlineData(-10, -5, 1, Pagination.DefaultPageSize)]
    [InlineData(2, 50, 2, 50)]
    [InlineData(3, 10_000, 3, Pagination.MaxPageSize)]
    [InlineData(int.MaxValue, 10, Pagination.MaxPageNumber, 10)]
    public void Normalize_enforces_safe_page_boundaries(
        int requestedPage,
        int requestedSize,
        int expectedPage,
        int expectedSize)
    {
        Pagination.Normalize(ref requestedPage, ref requestedSize);

        Assert.Equal(expectedPage, requestedPage);
        Assert.Equal(expectedSize, requestedSize);
    }

    [Fact]
    public void Paged_result_uses_normalized_values_for_metadata()
    {
        var result = PagedResult<int>.Create([1, 2], totalCount: 250, pageNumber: 0, pageSize: 1_000);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(Pagination.MaxPageSize, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
}
