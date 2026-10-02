using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FougeraClub1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureImagePathToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureImagePath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureImagePath",
                table: "AspNetUsers");
        }
    }
}
