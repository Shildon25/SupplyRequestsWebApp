using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyManagement.WebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedManyToManyForItemsAndRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_SupplyRequests_SupplyRequestId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "AccountRequests");

            migrationBuilder.DropIndex(
                name: "IX_Items_SupplyRequestId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SupplyRequestId",
                table: "Items");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSupplyRequest");

            migrationBuilder.AddColumn<int>(
                name: "SupplyRequestId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RequestedAccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRequests_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountRequests_AspNetUsers_RequestedAccountId",
                        column: x => x.RequestedAccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_SupplyRequestId",
                table: "Items",
                column: "SupplyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRequests_ApprovedByUserId",
                table: "AccountRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRequests_RequestedAccountId",
                table: "AccountRequests",
                column: "RequestedAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_SupplyRequests_SupplyRequestId",
                table: "Items",
                column: "SupplyRequestId",
                principalTable: "SupplyRequests",
                principalColumn: "Id");
        }
    }
}
