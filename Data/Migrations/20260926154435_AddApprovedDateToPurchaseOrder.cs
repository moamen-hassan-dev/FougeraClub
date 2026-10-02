using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FougeraClub1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedDateToPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "purchaseOrders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "purchaseOrders");
        }
    }
}
