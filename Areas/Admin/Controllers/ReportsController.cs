using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportHub.Data;
using ReportHub.Models;

namespace ReportHub.Areas.Admin.Controllers;

/// <summary>
/// 管理端报表维护
/// </summary>
[Area("Admin")]
[Authorize]
public class ReportsController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(AppDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 报表列表：包含已停用报表，可按分类筛选与名称搜索
    /// </summary>
    public async Task<IActionResult> Index(string? q, int? categoryId)
    {
        ViewData["Title"] = "报表管理";
        ViewData["SearchTerm"] = q;
        ViewData["CategoryId"] = categoryId;

        try
        {
            ViewBag.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .ToListAsync();

            var query = _context.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(r => r.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(r => r.Name.Contains(term) || (r.Keywords != null && r.Keywords.Contains(term)));
            }

            var reports = await query
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.Id)
                .ToListAsync();

            return View(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载报表管理列表失败");
            ViewBag.DataUnavailable = true;
            ViewBag.Categories = new List<Category>();
            return View(new List<Report>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "新增报表";
        await LoadCategorySelectAsync();
        return View(new Report());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Report report)
    {
        ViewData["Title"] = "新增报表";

        if (!await CategoryExistsAsync(report.CategoryId))
        {
            ModelState.AddModelError(nameof(Report.CategoryId), "请选择有效的分类");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategorySelectAsync(report.CategoryId);
            return View(report);
        }

        report.Id = 0;
        report.Category = null;
        report.Name = report.Name.Trim();
        report.ReportUrl = report.ReportUrl.Trim();

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"报表「{report.Name}」已创建";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var report = await _context.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (report is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "编辑报表";
        await LoadCategorySelectAsync(report.CategoryId);
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Report report)
    {
        ViewData["Title"] = "编辑报表";

        if (id != report.Id)
        {
            return NotFound();
        }

        if (!await CategoryExistsAsync(report.CategoryId))
        {
            ModelState.AddModelError(nameof(Report.CategoryId), "请选择有效的分类");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategorySelectAsync(report.CategoryId);
            return View(report);
        }

        var entity = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = report.Name.Trim();
        entity.ReportUrl = report.ReportUrl.Trim();
        entity.CategoryId = report.CategoryId;
        entity.Description = report.Description?.Trim();
        entity.Keywords = report.Keywords?.Trim();
        entity.IsActive = report.IsActive;
        entity.SortOrder = report.SortOrder;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"报表「{entity.Name}」已保存";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 启用 / 停用报表
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var entity = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();

        TempData["Success"] = entity.IsActive ? $"报表「{entity.Name}」已启用" : $"报表「{entity.Name}」已停用";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.Reports
            .AsNoTracking()
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "删除报表";
        return View(report);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        _context.Reports.Remove(entity);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"报表「{entity.Name}」已删除";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    private async Task LoadCategorySelectAsync(int? selectedId = null)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();

        ViewBag.Categories = new SelectList(categories, nameof(Category.Id), nameof(Category.Name), selectedId);
    }
}
