using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace geoback.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // SAFE MIGRATION - Only adds missing columns
            // ============================================
            
            // Add IsActive column to Users table if it doesn't exist
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tablename = 'Users';
                SET @columnname = 'IsActive';
                
                SET @preparedStatement = (SELECT IF(
                  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_SCHEMA = @dbname 
                   AND TABLE_NAME = @tablename 
                   AND COLUMN_NAME = @columnname) > 0,
                  'SELECT 1',
                  CONCAT('ALTER TABLE ', @tablename, ' ADD COLUMN ', @columnname, ' BOOLEAN NOT NULL DEFAULT TRUE;')
                ));
                
                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Add LastLoginAt column to Users table if it doesn't exist
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tablename = 'Users';
                SET @columnname = 'LastLoginAt';
                
                SET @preparedStatement = (SELECT IF(
                  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_SCHEMA = @dbname 
                   AND TABLE_NAME = @tablename 
                   AND COLUMN_NAME = @columnname) > 0,
                  'SELECT 1',
                  CONCAT('ALTER TABLE ', @tablename, ' ADD COLUMN ', @columnname, ' DATETIME NULL;')
                ));
                
                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Add index on Email for faster lookups
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tablename = 'Users';
                SET @indexname = 'IX_Users_Email';
                
                SET @preparedStatement = (SELECT IF(
                  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                   WHERE TABLE_SCHEMA = @dbname 
                   AND TABLE_NAME = @tablename 
                   AND INDEX_NAME = @indexname) > 0,
                  'SELECT 1',
                  CONCAT('CREATE INDEX ', @indexname, ' ON ', @tablename, ' (Email);')
                ));
                
                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Create __EFMigrationsHistory table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
                    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                    PRIMARY KEY (`MigrationId`)
                ) CHARACTER SET=utf8mb4;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional: Remove columns if rolling back
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tablename = 'Users';
                
                -- Drop IsActive column if it exists
                SET @preparedStatement = (SELECT IF(
                  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_SCHEMA = @dbname 
                   AND TABLE_NAME = @tablename 
                   AND COLUMN_NAME = 'IsActive') > 0,
                  'ALTER TABLE Users DROP COLUMN IsActive;',
                  'SELECT 1'
                ));
                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                
                -- Drop LastLoginAt column if it exists
                SET @preparedStatement = (SELECT IF(
                  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_SCHEMA = @dbname 
                   AND TABLE_NAME = @tablename 
                   AND COLUMN_NAME = 'LastLoginAt') > 0,
                  'ALTER TABLE Users DROP COLUMN LastLoginAt;',
                  'SELECT 1'
                ));
                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                
                -- Drop index if it exists
                SET @indexname = 'IX_Users_Email';
                SET @preparedStatement = (SELECT IF(
                  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                   WHERE TABLE_SCHEMA = @dbname 
                   AND TABLE_NAME = @tablename 
                   AND INDEX_NAME = @indexname) > 0,
                  CONCAT('DROP INDEX ', @indexname, ' ON ', @tablename, ';'),
                  'SELECT 1'
                ));
                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}