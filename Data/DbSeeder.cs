using Microsoft.EntityFrameworkCore;
using ReportHub.Models;

namespace ReportHub.Data;

/// <summary>
/// 数据库种子数据初始化
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// 初始化演示数据（仅在数据库为空时执行）。
    /// 需先执行 EF Core 迁移建表，再调用本方法。
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // 如果已有分类数据，说明已初始化过，跳过
        if (await context.Categories.AnyAsync())
            return;

        // ===== 演示分类 =====
        var categories = new[]
        {
            new Category { Name = "销售报表", Description = "销售业务相关报表", SortOrder = 1 },
            new Category { Name = "财务报表", Description = "财务核算与分析报表", SortOrder = 2 },
            new Category { Name = "人事报表", Description = "人力资源相关报表", SortOrder = 3 },
            new Category { Name = "库存报表", Description = "库存管理与分析报表", SortOrder = 4 },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // ===== 演示报表 =====
        var reports = new[]
        {
            new Report
            {
                Name = "月度销售额统计",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=sales_monthly.cpt",
                CategoryId = categories[0].Id,
                Description = "按月统计各部门销售额，支持同比环比对比",
                Keywords = "销售,月度,统计,收入",
                IsActive = true,
                SortOrder = 1
            },
            new Report
            {
                Name = "客户排名分析",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=customer_rank.cpt",
                CategoryId = categories[0].Id,
                Description = "按客户维度排名展示交易额及订单量",
                Keywords = "客户,排名,交易",
                IsActive = true,
                SortOrder = 2
            },
            new Report
            {
                Name = "利润表",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=profit_statement.cpt",
                CategoryId = categories[1].Id,
                Description = "季度利润表，含收入、成本、费用明细",
                Keywords = "利润,财务,季度",
                IsActive = true,
                SortOrder = 1
            },
            new Report
            {
                Name = "资产负债表",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=balance_sheet.cpt",
                CategoryId = categories[1].Id,
                Description = "资产负债表，展示期末财务状况",
                Keywords = "资产,负债,财务",
                IsActive = true,
                SortOrder = 2
            },
            new Report
            {
                Name = "员工花名册",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=employee_roster.cpt",
                CategoryId = categories[2].Id,
                Description = "在职员工基本信息一览",
                Keywords = "员工,花名册,人事",
                IsActive = true,
                SortOrder = 1
            },
            new Report
            {
                Name = "库存周转率分析",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=inventory_turnover.cpt",
                CategoryId = categories[3].Id,
                Description = "分析各品类库存周转率及滞销品",
                Keywords = "库存,周转,滞销",
                IsActive = true,
                SortOrder = 1
            },
            new Report
            {
                Name = "库存预警报表",
                ReportUrl = "http://localhost:8075/webroot/reportserver?reportlet=inventory_alert.cpt",
                CategoryId = categories[3].Id,
                Description = "低于安全库存的物料预警（已停用示例）",
                Keywords = "库存,预警,安全",
                IsActive = false,
                SortOrder = 2
            },
        };

        await context.Reports.AddRangeAsync(reports);
        await context.SaveChangesAsync();
    }
}
