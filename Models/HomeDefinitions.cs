namespace ReportHub.Models;

/// <summary>
/// 首页定义：统一管理有哪些独立首页、各自的编码、展示名称、Hero 标题
/// 新增首页只需在这里加一行 + 导航栏加一项 + 数据库对应分类/报表填新 HomeCode
/// </summary>
public static class HomeDefinitions
{
    /// <summary>帆软报表首页</summary>
    public static readonly HomeDef Fine = new(
        Code: "FINE",
        DisplayName: "点检报表",
        HeroTitle: "无纸化报表导航中心");

    /// <summary>生产报表首页（预留，数据需在管理后台添加）</summary>
    public static readonly HomeDef Production = new(
        Code: "PROD",
        DisplayName: "生产报表",
        HeroTitle: "生产报表导航中心");

    /// <summary>所有首页，按固定顺序展示在导航栏</summary>
    public static IReadOnlyList<HomeDef> All { get; } = [Fine, Production];

    /// <summary>默认首页编码（旧数据回填、路由默认值）</summary>
    public const string DefaultCode = "FINE";

    public static HomeDef? Find(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : All.FirstOrDefault(h => h.Code == code);
}

public sealed record HomeDef(string Code, string DisplayName, string HeroTitle);
