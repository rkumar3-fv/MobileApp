﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FreedomVoice.DAL.Migrations
{
    public partial class SendingStateForMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "Message",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Message",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Message");
        }
    }
}
