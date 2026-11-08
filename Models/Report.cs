using System.ComponentModel.DataAnnotations;

namespace ReportHub.Models;

/// <summary>
/// 报表信息
/// </summary>
public class Report
{
    public int Id { get; set; }

    /// <summary>
    /// 所属首页编码（FINE=帆软、PROD=生产），用于隔离不同首页的报表
    /// </summary>
    [Required(ErrorMessage = "首页编码不能为空")]
    [StringLength(50, ErrorMessage = "首页编码不能超过 50 个字符")]
    public string HomeCode { get; set; } = HomeDefinitions.Fine.Code;

    [Required(ErrorMessage = "报表名称不能为空")]
    [StringLength(100, ErrorMessage = "报表名称不能超过 100 个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "报表 URL 不能为空")]
    [StringLength(500, ErrorMessage = "报表 URL 不能超过 500 个字符")]
    public string ReportUrl { get; set; } = string.Empty;

    /// <summary>
    /// 所属分类 ID
    /// </summary>
    [Required(ErrorMessage = "请选择所属分类")]
    [Range(1, int.MaxValue, ErrorMessage = "请选择所属分类")]
    public int CategoryId { get; set; }

    // 导航属性
    public Category? Category { get; set; }

    [StringLength(500, ErrorMessage = "报表说明不能超过 500 个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 搜索关键词，逗号分隔
    /// </summary>
    [StringLength(200, ErrorMessage = "搜索关键词不能超过 200 个字符")]
    public string? Keywords { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序号，数值越小越靠前
    /// </summary>
    public int SortOrder { get; set; }
}
