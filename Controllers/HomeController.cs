using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReportHub.Models;
using ReportHub.Services;
using ReportHub.ViewModels;

namespace ReportHub.Controllers;

public class HomeController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IReportService reportService, ILogger<HomeController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// 报表首页：展示指定首页的已启用报表，支持关键词搜索与分类筛选（可同时使用）
    /// 默认首页编码 FINE 保持向后兼容（现有帆软数据全在 FINE 下）
    /// </summary>
    /// <param name="homeCode">首页编码，如 FINE / PROD；空或无效时回退到 FINE</param>
    /// <param name="q">搜索关键词</param>
    /// <param name="categoryId">分类 Id</param>
    public async Task<IActionResult> Index(string? homeCode, string? q, int? categoryId)
    {
        var currentHome = HomeDefinitions.Find(homeCode) ?? HomeDefinitions.Fine;

        ViewData["CurrentHomeCode"] = currentHome.Code;

        var model = new HomeIndexViewModel
        {
            HomeCode = currentHome.Code,
            SearchTerm = q?.Trim(),
            CategoryId = categoryId
        };

        try
        {
            model.Categories = await _reportService.GetCategoriesAsync(currentHome.Code);
            model.Reports = await _reportService.GetActiveReportsAsync(currentHome.Code, model.SearchTerm, categoryId);
        }
        catch (Exception ex)
        {
            // 数据库未配置或连接失败时，页面给出友好提示而不是抛出 500
            _logger.LogError(ex, "加载报表首页数据失败（HomeCode={HomeCode}）", currentHome.Code);
            model.DataUnavailable = true;
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
