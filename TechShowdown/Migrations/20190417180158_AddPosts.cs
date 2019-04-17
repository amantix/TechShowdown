using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TechShowdown.Migrations
{
    public partial class AddPosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "UserId" },
                values: new object[] { new Guid("880200b0-21fc-4a48-8588-12f0556df6d4"), "Winter is coming", new Guid("b32b790d-f9cb-49eb-9e7b-4e6132592084") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "UserId" },
                values: new object[] { new Guid("62b935d6-6d3e-4aeb-97c9-0242645b751b"), "Hasta la vista, baby", new Guid("62ad51cc-7475-48bb-b7a8-afe5631cd26f") });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
