using ReportHub.Models;

namespace ReportHub.ViewModels;

/// <summary>
/// 报表首页视图模型
/// </summary>
public class HomeIndexViewModel
{
    /// <summary>
    /// 当前首页编码
    /// </summary>
    public string HomeCode { get; set; } = HomeDefinitions.DefaultCode;

    /// <summary>
    /// 当前首页定义（标题、展示名等）
    /// </summary>
    public HomeDef CurrentHome => HomeDefinitions.Find(HomeCode) ?? HomeDefinitions.Fine;

    /// <summary>
    /// 当前筛选条件下的启用报表
    /// </summary>
    public IReadOnlyList<Report> Reports { get; set; } = [];

    /// <summary>
    /// 当前首页的全部分类（用于分类导航）
    /// </summary>
    public IReadOnlyList<Category> Categories { get; set; } = [];

    /// <summary>
    /// 当前搜索关键词
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 当前选中的分类 Id
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// 是否为初始状态（无搜索、无分类筛选）
    /// </summary>
    public bool HasFilter => !string.IsNullOrWhiteSpace(SearchTerm) || (CategoryId.HasValue && CategoryId.Value > 0);

    /// <summary>
    /// 数据库暂时不可用（未配置连接或连接失败）
    /// </summary>
    public bool DataUnavailable { get; set; }

    /// <summary>
    /// 当前选中分类的名称
    /// </summary>
    public string? CurrentCategoryName =>
        CategoryId.HasValue ? Categories.FirstOrDefault(c => c.Id == CategoryId.Value)?.Name : null;
}
