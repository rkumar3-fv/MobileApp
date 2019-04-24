using Microsoft.EntityFrameworkCore.Migrations;

namespace FreedomVoice.DAL.Migrations
{
    public partial class RenameMessagesRecievedAtToCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReceivedAt",
                table: "Message",
                newName: "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Message",
                newName: "ReceivedAt");
        }
    }
}
