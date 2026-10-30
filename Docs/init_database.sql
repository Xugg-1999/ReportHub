-- =====================================================================
-- ReportHub 数据库初始化脚本（MySQL 8.0+）
-- =====================================================================
-- 用途：手动建库建表（与 EF Core 迁移 InitialCreate 完全一致）
--
-- 使用方式（二选一）：
--   方式 A：直接执行本脚本建库建表（本脚本可重复执行，已存在则跳过）
--   方式 B：不执行本脚本，改用命令 dotnet ef database update 自动建表
--
-- 注意：
--   1. 本脚本包含 __EFMigrationsHistory 表并登记了 InitialCreate 记录，
--      因此用本脚本建表后，以后再执行 dotnet ef database update 时
--      不会重复执行 InitialCreate，只会应用后续新增的迁移。
--   2. 管理员账号不要用 SQL 手工插入（密码是 Identity 的 PBKDF2 哈希），
--      正确做法：配置 AdminSeed:UserName / AdminSeed:Password 后重启应用，
--      程序启动时会自动创建角色 Admin 和管理员账号。
--   3. 第 5 步演示数据建议只在空库时执行一次。
-- =====================================================================

-- ---------------------------------------------------------------------
-- 第 0 步：创建数据库（已存在可跳过）
-- ---------------------------------------------------------------------
CREATE DATABASE IF NOT EXISTS `reporthub`
    DEFAULT CHARACTER SET utf8mb4
    DEFAULT COLLATE utf8mb4_unicode_ci;

USE `reporthub`;

-- ---------------------------------------------------------------------
-- 第 1 步：EF 迁移历史表（必须保留，供 dotnet-ef 使用）
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId`     varchar(150) NOT NULL COMMENT '迁移名称',
    `ProductVersion`  varchar(32)  NOT NULL COMMENT 'EF Core 版本',
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='EF Core 迁移历史';

-- ---------------------------------------------------------------------
-- 第 2 步：业务表
-- ---------------------------------------------------------------------

-- 2.1 报表分类表
CREATE TABLE IF NOT EXISTS `Categories` (
    `Id`          int          NOT NULL AUTO_INCREMENT COMMENT '主键，自增',
    `Name`        varchar(50)  NOT NULL                COMMENT '分类名称（唯一）',
    `Description` varchar(200) NULL                    COMMENT '分类说明',
    `SortOrder`   int          NOT NULL DEFAULT 0      COMMENT '排序号，越小越靠前',
    CONSTRAINT `PK_Categories` PRIMARY KEY (`Id`),
    CONSTRAINT `IX_Categories_Name` UNIQUE (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='报表分类';

-- 2.2 帆软报表信息表
CREATE TABLE IF NOT EXISTS `Reports` (
    `Id`          int          NOT NULL AUTO_INCREMENT COMMENT '主键，自增',
    `Name`        varchar(100) NOT NULL                COMMENT '报表名称',
    `ReportUrl`   varchar(500) NOT NULL                COMMENT '帆软报表 URL',
    `CategoryId`  int          NOT NULL                COMMENT '所属分类 Id（外键 → Categories.Id）',
    `Description` varchar(500) NULL                    COMMENT '报表说明',
    `Keywords`    varchar(200) NULL                    COMMENT '搜索关键词，英文逗号分隔',
    `IsActive`    tinyint(1)   NOT NULL DEFAULT 1      COMMENT '是否启用：1=启用（首页可见）0=停用',
    `SortOrder`   int          NOT NULL DEFAULT 0      COMMENT '排序号，越小越靠前',
    CONSTRAINT `PK_Reports` PRIMARY KEY (`Id`),
    INDEX `IX_Reports_CategoryId` (`CategoryId`),
    CONSTRAINT `FK_Reports_Categories_CategoryId`
        FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='帆软报表信息';

-- ---------------------------------------------------------------------
-- 第 3 步：Identity 表（管理员账号体系，共 7 张）
-- 说明：由 ASP.NET Core Identity 框架使用，字段名/结构为框架约定，勿随意改动
-- ---------------------------------------------------------------------

-- 3.1 角色表（本项目固定使用角色 Admin）
CREATE TABLE IF NOT EXISTS `AspNetRoles` (
    `Id`               varchar(255) NOT NULL COMMENT '角色 Id（GUID 字符串）',
    `Name`             varchar(256) NULL     COMMENT '角色显示名',
    `NormalizedName`   varchar(256) NULL     COMMENT '角色名大写规范化（唯一索引）',
    `ConcurrencyStamp` longtext     NULL     COMMENT '并发戳',
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`),
    UNIQUE INDEX `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 角色';

-- 3.2 用户表（管理员账号存这里，PasswordHash 由 Identity 生成）
CREATE TABLE IF NOT EXISTS `AspNetUsers` (
    `Id`                   varchar(255) NOT NULL COMMENT '用户 Id（GUID 字符串）',
    `UserName`             varchar(256) NULL     COMMENT '用户名',
    `NormalizedUserName`   varchar(256) NULL     COMMENT '用户名大写规范化（唯一索引）',
    `Email`                varchar(256) NULL     COMMENT '邮箱',
    `NormalizedEmail`      varchar(256) NULL     COMMENT '邮箱大写规范化（索引）',
    `EmailConfirmed`       tinyint(1)   NOT NULL COMMENT '邮箱是否已确认',
    `PasswordHash`         longtext     NULL     COMMENT '密码哈希（PBKDF2，禁止手工构造）',
    `SecurityStamp`        longtext     NULL     COMMENT '安全戳（改密码/登出全部会话用）',
    `ConcurrencyStamp`     longtext     NULL     COMMENT '并发戳',
    `PhoneNumber`          longtext     NULL     COMMENT '手机号',
    `PhoneNumberConfirmed` tinyint(1)   NOT NULL COMMENT '手机号是否已确认',
    `TwoFactorEnabled`     tinyint(1)   NOT NULL COMMENT '是否开启两步验证',
    `LockoutEnd`           datetime(6)  NULL     COMMENT '锁定截止时间（NULL=未锁定）',
    `LockoutEnabled`       tinyint(1)   NOT NULL COMMENT '是否允许锁定',
    `AccessFailedCount`    int          NOT NULL COMMENT '连续登录失败次数',
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`),
    INDEX `EmailIndex` (`NormalizedEmail`),
    UNIQUE INDEX `UserNameIndex` (`NormalizedUserName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 用户（管理员）';

-- 3.3 用户-角色关联表
CREATE TABLE IF NOT EXISTS `AspNetUserRoles` (
    `UserId` varchar(255) NOT NULL COMMENT '用户 Id（外键 → AspNetUsers.Id）',
    `RoleId` varchar(255) NOT NULL COMMENT '角色 Id（外键 → AspNetRoles.Id）',
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    INDEX `IX_AspNetUserRoles_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId`
        FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 用户角色关联';

-- 3.4 用户声明表
CREATE TABLE IF NOT EXISTS `AspNetUserClaims` (
    `Id`         int          NOT NULL AUTO_INCREMENT COMMENT '主键，自增',
    `UserId`     varchar(255) NOT NULL                COMMENT '用户 Id（外键 → AspNetUsers.Id）',
    `ClaimType`  longtext     NULL                    COMMENT '声明类型',
    `ClaimValue` longtext     NULL                    COMMENT '声明值',
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    INDEX `IX_AspNetUserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 用户声明';

-- 3.5 用户第三方登录表
CREATE TABLE IF NOT EXISTS `AspNetUserLogins` (
    `LoginProvider`       varchar(255) NOT NULL COMMENT '登录提供方（如 Google）',
    `ProviderKey`         varchar(255) NOT NULL COMMENT '提供方侧的用户唯一键',
    `ProviderDisplayName` longtext     NULL     COMMENT '提供方显示名',
    `UserId`              varchar(255) NOT NULL COMMENT '用户 Id（外键 → AspNetUsers.Id）',
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    INDEX `IX_AspNetUserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 第三方登录';

-- 3.6 用户令牌表
CREATE TABLE IF NOT EXISTS `AspNetUserTokens` (
    `UserId`        varchar(255) NOT NULL COMMENT '用户 Id（外键 → AspNetUsers.Id）',
    `LoginProvider` varchar(255) NOT NULL COMMENT '令牌提供方',
    `Name`          varchar(255) NOT NULL COMMENT '令牌名称',
    `Value`         longtext     NULL     COMMENT '令牌值',
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 用户令牌';

-- 3.7 角色声明表
CREATE TABLE IF NOT EXISTS `AspNetRoleClaims` (
    `Id`         int          NOT NULL AUTO_INCREMENT COMMENT '主键，自增',
    `RoleId`     varchar(255) NOT NULL                COMMENT '角色 Id（外键 → AspNetRoles.Id）',
    `ClaimType`  longtext     NULL                    COMMENT '声明类型',
    `ClaimValue` longtext     NULL                    COMMENT '声明值',
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    INDEX `IX_AspNetRoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId`
        FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Identity 角色声明';

-- ---------------------------------------------------------------------
-- 第 4 步：登记迁移记录（用本脚本建表时必须执行，
--          否则以后 dotnet ef database update 会重复建表报错）
-- ---------------------------------------------------------------------
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261023054731_InitialCreate', '9.0.0');

-- ---------------------------------------------------------------------
-- 第 5 步（可选）：初始角色 + 演示业务数据
-- 说明：仅当 Categories 为空时才需要执行演示数据；
--       管理员账号由应用启动时自动创建（AdminSeed 配置），不要手工插用户。
-- ---------------------------------------------------------------------

-- 5.1 Admin 角色（Id 固定，便于后续关联）
INSERT IGNORE INTO `AspNetRoles` (`Id`, `Name`, `NormalizedName`, `ConcurrencyStamp`)
VALUES ('admin-role-0000000000000000000000000001', 'Admin', 'ADMIN', UUID());

-- 5.2 演示分类（与 DbSeeder 一致；空库时执行，已有数据请跳过本段）
INSERT INTO `Categories` (`Name`, `Description`, `SortOrder`) VALUES
    ('销售报表', '销售业务相关报表', 1),
    ('财务报表', '财务核算与分析报表', 2),
    ('人事报表', '人力资源相关报表', 3),
    ('库存报表', '库存管理与分析报表', 4);

-- 5.3 演示报表（依赖上面分类自增 Id 从 1 开始；空库时执行，已有数据请跳过本段）
INSERT INTO `Reports` (`Name`, `ReportUrl`, `CategoryId`, `Description`, `Keywords`, `IsActive`, `SortOrder`) VALUES
    ('月度销售额统计', 'http://localhost:8075/webroot/reportserver?reportlet=sales_monthly.cpt',      1, '按月统计各部门销售额，支持同比环比对比', '销售,月度,统计,收入', 1, 1),
    ('客户排名分析',   'http://localhost:8075/webroot/reportserver?reportlet=customer_rank.cpt',      1, '按客户维度排名展示交易额及订单量',       '客户,排名,交易',       1, 2),
    ('利润表',         'http://localhost:8075/webroot/reportserver?reportlet=profit_statement.cpt',   2, '季度利润表，含收入、成本、费用明细',     '利润,财务,季度',       1, 1),
    ('资产负债表',     'http://localhost:8075/webroot/reportserver?reportlet=balance_sheet.cpt',      2, '资产负债表，展示期末财务状况',           '资产,负债,财务',       1, 2),
    ('员工花名册',     'http://localhost:8075/webroot/reportserver?reportlet=employee_roster.cpt',    3, '在职员工基本信息一览',                   '员工,花名册,人事',     1, 1),
    ('库存周转率分析', 'http://localhost:8075/webroot/reportserver?reportlet=inventory_turnover.cpt', 4, '分析各品类库存周转率及滞销品',           '库存,周转,滞销',       1, 1),
    ('库存预警报表',   'http://localhost:8075/webroot/reportserver?reportlet=inventory_alert.cpt',    4, '低于安全库存的物料预警（已停用示例）',   '库存,预警,安全',       0, 2);
