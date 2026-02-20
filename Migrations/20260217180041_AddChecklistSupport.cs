using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace geoback.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `Checklists` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `DclNo` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CustomerId` longtext CHARACTER SET utf8mb4 NULL,
    `CustomerNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CustomerName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CustomerEmail` longtext CHARACTER SET utf8mb4 NULL,
    `LoanType` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IbpsNo` longtext CHARACTER SET utf8mb4 NULL,
    `AssignedToRM` char(36) COLLATE ascii_general_ci NULL,
    `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DocumentsJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Checklists` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checklists");
        }
    }
}
