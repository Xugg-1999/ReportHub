# ReportHub

企业内部帆软报表导航网站。提供分类浏览、关键词搜索、管理员维护入口，让同事在一个页面里找到并打开有权限访问的帆软报表。

## 功能

- **报表首页**：Hero 搜索区 + 分类标签导航 + 响应式卡片网格，支持关键词搜索和分类筛选
- **管理后台**（需登录）：
  - 报表维护：列表 / 新增 / 编辑 / 启用停用 / 删除
  - 分类维护：列表 / 新增 / 编辑 / 删除（分类被报表引用时禁止删除）
- **管理员登录**：ASP.NET Core Identity，初始账号由配置项自动创建

## 技术栈

| 组件 | 版本 |
|---|---|
| .NET SDK | 9.0+ |
| ASP.NET Core MVC | net9.0 |
| Entity Framework Core | 9.0.0 |
| MySQL（Pomelo） | 9.0.0 |
| ASP.NET Core Identity | 9.0.0（内置） |
| Bootstrap | 5（内置静态资源） |

## 快速开始

### 1. 安装依赖

```powershell
dotnet restore
```

### 2. 准备数据库

本机 MySQL 8.0+，建库建表二选一：

**方式 A：执行脚本**（脚本包含 EF 迁移记录，后续 `dotnet ef database update` 不会重复建表）

```bash
mysql -u root -p < Docs/init_database.sql
```

**方式 B：交给 EF 迁移**

```powershell
# 先在 appsettings.Development.json 里填好连接字符串，然后：
dotnet ef database update
```

两种方式二选一，脚本路径：[`Docs/init_database.sql`](Docs/init_database.sql)。

### 3. 填写连接字符串

编辑 `appsettings.Development.json`（开发环境）或设置环境变量 `ConnectionStrings__DefaultConnection`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=reporthub;User=root;Password=你的密码;CharSet=utf8mb4;"
  }
}
```

数据库连接未就绪时应用也能启动（占位连接），只是首页会显示"数据暂不可用"提示。

### 4. 配置管理员账号

在 `appsettings.Development.json` 的 `AdminSeed` 节填用户名密码，启动时自动创建角色 `Admin` 和账号。**不要在源码里提交真实密码**——建议只在本地 Development 配置里写，或改用环境变量 `AdminSeed__UserName` / `AdminSeed__Password`。

```json
{
  "AdminSeed": {
    "UserName": "admin",
    "Password": "Admin@12345"
  }
}
```

密码需满足 Identity 默认策略：≥6 位，含大小写字母、数字、符号。

### 5. 运行

```powershell
dotnet run --launch-profile http
```

浏览器打开 http://localhost:5105

| 入口 | 地址 |
|---|---|
| 报表首页 | / |
| 管理员登录 | /Account/Login |
| 报表管理 | /Admin/Reports（需登录） |
| 分类管理 | /Admin/Categories（需登录） |

### 6. 构建

```powershell
dotnet build
```

## 数据库表

业务表 2 张，Identity 表 7 张，迁移历史表 1 张，总计 10 张：

| 表 | 类型 | 说明 |
|---|---|---|
| `Categories` | 业务 | 报表分类 |
| `Reports` | 业务 | 帆软报表，外键 → Categories，级联删除禁止 |
| `AspNetUsers` | Identity | 管理员账号 |
| `AspNetRoles` | Identity | 角色（固定使用 `Admin`） |
| `AspNetUserRoles` | Identity | 用户 ↔ 角色关联 |
| `AspNetUserClaims` / `AspNetRoleClaims` | Identity | 声明表 |
| `AspNetUserLogins` | Identity | 第三方登录 |
| `AspNetUserTokens` | Identity | 用户令牌 |
| `__EFMigrationsHistory` | 框架 | EF 迁移记录 |

表结构定义详见 [`Docs/init_database.sql`](Docs/init_database.sql)，迁移快照在 [`Migrations/`](Migrations/) 目录。

## 目录结构

```
ReportHub/
├── Areas/Admin/           # 管理后台（Area）
│   ├── Controllers/       # ReportsController、CategoriesController
│   └── Views/             # 报表和分类的增删改查视图
├── Controllers/
│   ├── HomeController     # 首页（搜索 + 分类筛选）
│   └── AccountController  # 管理员登录 / 退出
├── Data/
│   ├── AppDbContext.cs    # EF 上下文，继承 IdentityDbContext<IdentityUser>
│   ├── AdminSeeder.cs     # 管理员账号初始化（读 AdminSeed 配置）
│   └── DbSeeder.cs        # 演示分类 + 报表种子（暂未自动调用）
├── Docs/
│   └── init_database.sql  # 手动建表脚本
├── Migrations/            # EF 迁移快照
├── Models/                # Category、Report 实体
├── Services/              # IReportService + ReportService（首页查询）
├── ViewModels/            # HomeIndexViewModel、LoginViewModel
├── Views/
│   ├── Account/Login.cshtml
│   ├── Home/Index.cshtml
│   └── Shared/_Layout.cshtml
├── wwwroot/css/site.css   # 全局样式（rh-* 前缀）
├── Program.cs             # 启动配置
└── ReportHub.csproj
```

## 常见问题

**Q：首页提示"数据库暂不可用"？**
A：检查连接字符串是否能连上 MySQL；改完重启即可。

**Q：登录时提示"数据库暂不可用，暂时无法登录"？**
A：同上，连接字符串未通，或数据库还没建表。

**Q：管理员登录总是失败？**
A：确认 `AdminSeed` 已配置且密码满足 Identity 策略；也可查 `AspNetUsers` 表里是否有记录。

**Q：新增报表时 URL 提示"请输入有效的 URL"？**
A：之前版本用了 `[Url]` 注解拦截，某些帆软含双重编码路径会被误判。当前版本已移除该注解，仅做必填 + 长度校验。

**Q：删除分类时报"还有 N 个报表，无法删除"？**
A：设计如此，避免孤立数据。先把分类下的报表移走或删掉。

## 后续可扩展

- 帆软单点登录（SSO）对接
- 报表访问统计（PV、UV、最近热门）
- 收藏 / 我的常用
- 定时同步帆软目录
- 多租户 / 多角色权限（当前单一管理员账号）
