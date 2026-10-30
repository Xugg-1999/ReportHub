using Microsoft.EntityFrameworkCore;
using ReportHub.Data;
using ReportHub.Models;

namespace ReportHub.Services;

/// <summary>
/// 报表查询服务实现
/// </summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<List<Report>> GetActiveReportsAsync(string? keyword, int? categoryId)
    {
        var query = _context.Reports
            .AsNoTracking()
            .Include(r => r.Category)
            .Where(r => r.IsActive);

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(r =>
                r.Name.Contains(term) ||
                (r.Keywords != null && r.Keywords.Contains(term)));
        }

        return await query
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Id)
            .ToListAsync();
    }
}
