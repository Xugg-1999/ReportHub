using ReportHub.Models;

namespace ReportHub.Services;

/// <summary>
/// 报表查询服务
/// </summary>
public interface IReportService
{
    /// <summary>
    /// 获取指定首页的全部分类（按排序号升序）
    /// </summary>
    Task<List<Category>> GetCategoriesAsync(string homeCode);

    /// <summary>
    /// 获取指定首页的已启用报表，支持按关键词和分类筛选（两者可同时使用）
    /// </summary>
    /// <param name="homeCode">所属首页编码</param>
    /// <param name="keyword">搜索关键词，匹配报表名称或搜索关键词字段</param>
    /// <param name="categoryId">分类 Id，为 null 或 0 时表示全部</param>
    Task<List<Report>> GetActiveReportsAsync(string homeCode, string? keyword, int? categoryId);
}
