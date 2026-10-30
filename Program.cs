using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReportHub.Data;
using ReportHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// MySQL 连接字符串当前留空，部署或本地调试时在 appsettings.Development.json
// 或环境变量 ConnectionStrings__DefaultConnection 中填写即可，无需修改代码
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    // 未配置时使用占位连接，保证项目可以正常启动查看界面
    connectionString = "Server=192.168.167.212;Port=3307;Database=reporthub;CharSet=utf8mb4;";
    //connectionString = "server=192.168.167.212;port=3307;database=reporthub;user=root;password=sgrs_passw0rd;charset=utf8;SslMode=Preferred;";
}

// 固定 MySQL 版本，避免设计时和运行时都需要连接数据库检测版本
var serverVersion = new MySqlServerVersion(new Version(8, 0, 42));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// 管理员登录：单一账号体系，只使用 Identity 默认用户表
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // 内部工具，密码策略保留默认强度（至少 6 位，含大小写、数字、符号）
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 登录 Cookie 安全配置
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// 注册业务服务
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// 管理员账号初始化：账号密码从配置 AdminSeed 读取，未配置则跳过；
// 数据库不可用时只记录警告，不阻断应用启动
using (var scope = app.Services.CreateScope())
{
    try
    {
        await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "管理员初始化失败（数据库可能尚未就绪），配置好连接并执行迁移后重启即可");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Reports}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
