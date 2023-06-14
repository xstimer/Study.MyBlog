﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Study.Net.EFCoreEnvironment.Migrations
{
    /// <inheritdoc />
    public partial class addJwtVersionInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "JwtVersion",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtVersion",
                table: "AspNetUsers");
        }
    }
}
