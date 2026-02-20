using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace geoback.Migrations
{
    /// <inheritdoc />
    public partial class AddClientCustomerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.Sql(@"
SET @dbname = DATABASE();
SET @tablename = 'Clients';
SET @columnname = 'CustomerId';

SET @preparedStatement = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = @dbname
     AND TABLE_NAME = @tablename
     AND COLUMN_NAME = @columnname) > 0,
    'SELECT 1',
    'ALTER TABLE `Clients` ADD `CustomerId` varchar(50) CHARACTER SET utf8mb4 NOT NULL DEFAULT '''';'
));

PREPARE stmt FROM @preparedStatement;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

                        migrationBuilder.Sql(@"
UPDATE `Clients`
SET `CustomerId` = `CustomerNumber`
WHERE (`CustomerId` IS NULL OR `CustomerId` = '')
    AND `CustomerNumber` IS NOT NULL
    AND `CustomerNumber` <> '';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.Sql(@"
SET @dbname = DATABASE();
SET @tablename = 'Clients';
SET @columnname = 'CustomerId';

SET @preparedStatement = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = @dbname
     AND TABLE_NAME = @tablename
     AND COLUMN_NAME = @columnname) > 0,
    'ALTER TABLE `Clients` DROP COLUMN `CustomerId`;',
    'SELECT 1'
));

PREPARE stmt FROM @preparedStatement;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}
