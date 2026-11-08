using System.ComponentModel.DataAnnotations;

namespace ReportHub.Models;

/// <summary>
/// 报表分类
/// </summary>
public class Category
{
    public int Id { get; set; }

    /// <summary>
    /// 所属首页编码（FINE=帆软、PROD=生产），用于隔离不同首页的分类
    /// </summary>
    [Required(ErrorMessage = "首页编码不能为空")]
    [StringLength(50, ErrorMessage = "首页编码不能超过 50 个字符")]
    public string HomeCode { get; set; } = HomeDefinitions.Fine.Code;

    [Required(ErrorMessage = "分类名称不能为空")]
    [StringLength(50, ErrorMessage = "分类名称不能超过 50 个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "分类说明不能超过 200 个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序号，数值越小越靠前
    /// </summary>
    public int SortOrder { get; set; }

    // 导航属性
    public ICollection<Report> Reports { get; set; } = [];
}
