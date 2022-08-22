using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluentFeeds.App.Shared.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedNodes",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentIdentifier = table.Column<Guid>(type: "TEXT", nullable: true),
                    HasChildren = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Feed = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Symbol = table.Column<int>(type: "INTEGER", nullable: true),
                    IsUserCustomizable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedNodes", x => x.Identifier);
                    table.ForeignKey(
                        name: "FK_FeedNodes_FeedNodes_ParentIdentifier",
                        column: x => x.ParentIdentifier,
                        principalTable: "FeedNodes",
                        principalColumn: "Identifier");
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    ContentUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PublishedTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ModifiedTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "FeedProviders",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    RootNodeIdentifier = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedProviders", x => x.Identifier);
                    table.ForeignKey(
                        name: "FK_FeedProviders_FeedNodes_RootNodeIdentifier",
                        column: x => x.RootNodeIdentifier,
                        principalTable: "FeedNodes",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedNodes_ParentIdentifier",
                table: "FeedNodes",
                column: "ParentIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_FeedProviders_RootNodeIdentifier",
                table: "FeedProviders",
                column: "RootNodeIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ProviderIdentifier_StorageIdentifier",
                table: "Items",
                columns: new[] { "ProviderIdentifier", "StorageIdentifier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedProviders");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "FeedNodes");
        }
    }
}
