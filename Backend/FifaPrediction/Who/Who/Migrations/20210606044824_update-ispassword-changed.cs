using Microsoft.EntityFrameworkCore.Migrations;

namespace Who.Migrations
{
    public partial class updateispasswordchanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPasswordChanged",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPasswordChanged",
                table: "AspNetUsers");
        }
    }
}
