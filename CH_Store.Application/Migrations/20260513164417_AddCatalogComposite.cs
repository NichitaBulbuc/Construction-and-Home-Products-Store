using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CH_Store.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogComposite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogKits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogKits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogKitItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KitId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    SubKitId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogKitItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogKitItems_CatalogKits_KitId",
                        column: x => x.KitId,
                        principalTable: "CatalogKits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogKitItems_CatalogKits_SubKitId",
                        column: x => x.SubKitId,
                        principalTable: "CatalogKits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CatalogKitItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogKitItems_KitId",
                table: "CatalogKitItems",
                column: "KitId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogKitItems_ProductId",
                table: "CatalogKitItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogKitItems_SubKitId",
                table: "CatalogKitItems",
                column: "SubKitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogKitItems");

            migrationBuilder.DropTable(
                name: "CatalogKits");
        }
    }
}
