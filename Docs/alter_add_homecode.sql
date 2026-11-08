-- =====================================================================
-- ReportHub 多首页支持增量脚本（MySQL 8.0+）
-- =====================================================================
-- 用途：给已建好的 Categories / Reports 表加 HomeCode 字段，
--       实现多个独立首页（帆软 FINE / 生产 PROD）的数据隔离
--
-- 前置条件：init_database.sql 已执行、Categories / Reports 表存在
-- 幂等：已加过的列会跳过（用 ADD COLUMN IF NOT EXISTS）
-- =====================================================================

USE `reporthub`;

-- ---------------------------------------------------------------------
-- 1. Categories 加 HomeCode
-- ---------------------------------------------------------------------
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Categories' AND COLUMN_NAME = 'HomeCode') = 0,
    'ALTER TABLE `Categories` ADD COLUMN `HomeCode` VARCHAR(50) NOT NULL DEFAULT ''FINE'' COMMENT ''所属首页编码：FINE=帆软、PROD=生产'' AFTER `Id`',
    'SELECT ''Categories.HomeCode 已存在，跳过''');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- 2. Reports 加 HomeCode
-- ---------------------------------------------------------------------
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Reports' AND COLUMN_NAME = 'HomeCode') = 0,
    'ALTER TABLE `Reports` ADD COLUMN `HomeCode` VARCHAR(50) NOT NULL DEFAULT ''FINE'' COMMENT ''所属首页编码：FINE=帆软、PROD=生产'' AFTER `Id`',
    'SELECT ''Reports.HomeCode 已存在，跳过''');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- 3. Categories 原 Name 唯一索引 → 复合 (HomeCode, Name) 唯一
--    不同首页下分类名称可以相同（各自独立），同首页内仍唯一
-- ---------------------------------------------------------------------
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Categories' AND INDEX_NAME = 'IX_Categories_Name') > 0,
    'ALTER TABLE `Categories` DROP INDEX `IX_Categories_Name`',
    'SELECT ''原 Categories 唯一索引不存在，跳过''');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Categories' AND INDEX_NAME = 'IX_Categories_HomeCode_Name') = 0,
    'ALTER TABLE `Categories` ADD UNIQUE INDEX `IX_Categories_HomeCode_Name` (`HomeCode`, `Name`)',
    'SELECT ''Categories 复合唯一索引已存在，跳过''');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- 4. Reports 加 (HomeCode, IsActive, SortOrder) 复合索引
--    覆盖首页查询的 WHERE + ORDER BY，提升性能
-- ---------------------------------------------------------------------
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Reports' AND INDEX_NAME = 'IX_Reports_HomeCode_IsActive_SortOrder') = 0,
    'ALTER TABLE `Reports` ADD INDEX `IX_Reports_HomeCode_IsActive_SortOrder` (`HomeCode`, `IsActive`, `SortOrder`)',
    'SELECT ''Reports 复合索引已存在，跳过''');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- 5. 现有数据显式回填 HomeCode = 'FINE'
--    （有 DEFAULT 会自动填，这里再写一次确保万无一失）
-- ---------------------------------------------------------------------
UPDATE `Categories` SET `HomeCode` = 'FINE' WHERE `HomeCode` IS NULL OR `HomeCode` = '';
UPDATE `Reports`    SET `HomeCode` = 'FINE' WHERE `HomeCode` IS NULL OR `HomeCode` = '';

-- ---------------------------------------------------------------------
-- 6. EF 迁移历史补一条占位（可选，保持 __EFMigrationsHistory 连续）
--    如果后续要用 dotnet ef database update 新增迁移，这条让它跳过本次
-- ---------------------------------------------------------------------
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261103000000_AddHomeCode', '9.0.0');

-- =====================================================================
-- 完成。之后管理后台新增的分类/报表，通过顶部首页选择器自动归属
-- 到对应 HomeCode（FINE 或 PROD），数据完全隔离。
-- =====================================================================
