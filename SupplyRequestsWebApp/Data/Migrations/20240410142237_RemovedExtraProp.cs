using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyManagement.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class RemovedExtraProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSupplyRequest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemSupplyRequest",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    SupplyRequestsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSupplyRequest", x => new { x.ItemsId, x.SupplyRequestsId });
                    table.ForeignKey(
                        name: "FK_ItemSupplyRequest_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSupplyRequest_SupplyRequests_SupplyRequestsId",
                        column: x => x.SupplyRequestsId,
                        principalTable: "SupplyRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplyRequest_SupplyRequestsId",
                table: "ItemSupplyRequest",
                column: "SupplyRequestsId");
        }
    }
}
