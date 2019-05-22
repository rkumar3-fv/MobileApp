using Microsoft.EntityFrameworkCore.Migrations;

namespace FreedomVoice.DAL.Migrations
{
    public partial class SendingStateForMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Message",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Message");
        }
    }
}
