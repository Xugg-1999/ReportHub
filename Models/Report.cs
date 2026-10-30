using System.ComponentModel.DataAnnotations;

namespace ReportHub.Models;

/// <summary>
/// 帆软报表信息
/// </summary>
public class Report
{
    public int Id { get; set; }

    [Required(ErrorMessage = "报表名称不能为空")]
    [StringLength(100, ErrorMessage = "报表名称不能超过 100 个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "帆软报表 URL 不能为空")]
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
