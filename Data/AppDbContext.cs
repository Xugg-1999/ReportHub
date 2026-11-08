using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReportHub.Models;

namespace ReportHub.Data;

/// <summary>
/// 应用数据库上下文（同时承载 Identity 的管理员账号表）
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category 配置
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HomeCode).HasMaxLength(50).IsRequired().HasDefaultValue(HomeDefinitions.DefaultCode);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            // 不同首页下分类名称可以相同（各自独立维护），同首页内唯一
            entity.HasIndex(e => new { e.HomeCode, e.Name }).IsUnique();
        });

        // Report 配置
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HomeCode).HasMaxLength(50).IsRequired().HasDefaultValue(HomeDefinitions.DefaultCode);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ReportUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Keywords).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            // 常用查询：按首页 + 启用 + 排序
            entity.HasIndex(e => new { e.HomeCode, e.IsActive, e.SortOrder });

            // 外键关系
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Reports)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
