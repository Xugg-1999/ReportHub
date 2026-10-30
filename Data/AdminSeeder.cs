using Microsoft.AspNetCore.Identity;

namespace ReportHub.Data;

/// <summary>
/// 管理员账号初始化：初始账号密码从配置读取，不在源码中写死
/// </summary>
public static class AdminSeeder
{
    private const string AdminRole = "Admin";

    /// <summary>
    /// 读取 AdminSeed:UserName / AdminSeed:Password，创建角色与管理员账号。
    /// 未配置时直接跳过；重复执行是幂等的。
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        var userName = configuration["AdminSeed:UserName"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("未配置 AdminSeed:UserName / AdminSeed:Password，跳过管理员初始化");
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new IdentityUser { UserName = userName, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogError("创建管理员 {UserName} 失败：{Errors}",
                    userName, string.Join("；", createResult.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, AdminRole))
        {
            await userManager.AddToRoleAsync(user, AdminRole);
        }

        logger.LogInformation("管理员账号 {UserName} 已就绪", userName);
    }
}
