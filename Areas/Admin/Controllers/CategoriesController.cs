using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportHub.Data;
using ReportHub.Models;

namespace ReportHub.Areas.Admin.Controllers;

/// <summary>
/// 管理端分类维护
/// </summary>
[Area("Admin")]
[Authorize]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? homeCode)
    {
        var currentHome = HomeDefinitions.Find(homeCode) ?? HomeDefinitions.Fine;

        ViewData["Title"] = "分类管理";
        ViewData["HomeCode"] = currentHome.Code;
        ViewData["HomeList"] = new SelectList(HomeDefinitions.All, nameof(HomeDef.Code), nameof(HomeDef.DisplayName), currentHome.Code);

        try
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Reports)
                .Where(c => c.HomeCode == currentHome.Code)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .ToListAsync();

            return View(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载分类管理列表失败（HomeCode={HomeCode}）", currentHome.Code);
            ViewBag.DataUnavailable = true;
            return View(new List<Category>());
        }
    }

    [HttpGet]
    public IActionResult Create(string? homeCode)
    {
        var currentHome = HomeDefinitions.Find(homeCode) ?? HomeDefinitions.Fine;
        ViewData["Title"] = "新增分类";
        ViewData["HomeCode"] = currentHome.Code;
        return View(new Category { HomeCode = currentHome.Code });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        var currentHome = HomeDefinitions.Find(category.HomeCode) ?? HomeDefinitions.Fine;
        category.HomeCode = currentHome.Code;

        ViewData["Title"] = "新增分类";
        ViewData["HomeCode"] = currentHome.Code;

        category.Name = category.Name.Trim();

        if (await _context.Categories.AnyAsync(c => c.HomeCode == currentHome.Code && c.Name == category.Name))
        {
            ModelState.AddModelError(nameof(Category.Name), "当前首页下已存在同名分类");
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        category.Id = 0;
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"分类「{category.Name}」已创建";
        return RedirectToAction(nameof(Index), new { homeCode = currentHome.Code });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "编辑分类";
        ViewData["HomeCode"] = category.HomeCode;
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        var entity = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "编辑分类";
        ViewData["HomeCode"] = entity.HomeCode;

        category.Name = category.Name.Trim();

        if (await _context.Categories.AnyAsync(c => c.HomeCode == entity.HomeCode && c.Name == category.Name && c.Id != id))
        {
            ModelState.AddModelError(nameof(Category.Name), "当前首页下已存在同名分类");
        }

        if (!ModelState.IsValid)
        {
            category.HomeCode = entity.HomeCode;
            return View(category);
        }

        entity.Name = category.Name;
        entity.Description = category.Description?.Trim();
        entity.SortOrder = category.SortOrder;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"分类「{entity.Name}」已保存";
        return RedirectToAction(nameof(Index), new { homeCode = entity.HomeCode });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "删除分类";
        ViewData["ReportCount"] = await _context.Reports.CountAsync(r => r.CategoryId == id);
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        // 有报表引用时不允许删除，避免产生孤立报表
        var reportCount = await _context.Reports.CountAsync(r => r.CategoryId == id);
        if (reportCount > 0)
        {
            TempData["Error"] = $"分类「{category.Name}」下还有 {reportCount} 个报表，无法删除。请先调整或删除这些报表。";
            return RedirectToAction(nameof(Index), new { homeCode = category.HomeCode });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"分类「{category.Name}」已删除";
        return RedirectToAction(nameof(Index), new { homeCode = category.HomeCode });
    }
}
