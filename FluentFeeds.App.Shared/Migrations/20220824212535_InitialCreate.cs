﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluentFeeds.App.Shared.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeletedItems",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserIdentifier = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedItems", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "FeedItems",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    FeedIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemIdentifier = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedItems", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "Feeds",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemStorageIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentIdentifier = table.Column<Guid>(type: "TEXT", nullable: true),
                    HasChildren = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentLoader = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Symbol = table.Column<int>(type: "INTEGER", nullable: true),
                    MetadataName = table.Column<string>(type: "TEXT", nullable: true),
                    MetadataAuthor = table.Column<string>(type: "TEXT", nullable: true),
                    MetadataDescription = table.Column<string>(type: "TEXT", nullable: true),
                    MetadataSymbol = table.Column<int>(type: "INTEGER", nullable: true),
                    IsUserCustomizable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsExcludedFromGroup = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feeds", x => x.Identifier);
                    table.ForeignKey(
                        name: "FK_Feeds_Feeds_ParentIdentifier",
                        column: x => x.ParentIdentifier,
                        principalTable: "Feeds",
                        principalColumn: "Identifier");
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    PublishedTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ModifiedTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    ContentUrl = table.Column<string>(type: "TEXT", nullable: true),
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
                        name: "FK_FeedProviders_Feeds_RootNodeIdentifier",
                        column: x => x.RootNodeIdentifier,
                        principalTable: "Feeds",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeletedItems_ProviderIdentifier_StorageIdentifier",
                table: "DeletedItems",
                columns: new[] { "ProviderIdentifier", "StorageIdentifier" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_FeedIdentifier",
                table: "FeedItems",
                column: "FeedIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_FeedProviders_RootNodeIdentifier",
                table: "FeedProviders",
                column: "RootNodeIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Feeds_ParentIdentifier",
                table: "Feeds",
                column: "ParentIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Feeds_ProviderIdentifier_ItemStorageIdentifier",
                table: "Feeds",
                columns: new[] { "ProviderIdentifier", "ItemStorageIdentifier" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ProviderIdentifier_StorageIdentifier",
                table: "Items",
                columns: new[] { "ProviderIdentifier", "StorageIdentifier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeletedItems");

            migrationBuilder.DropTable(
                name: "FeedItems");

            migrationBuilder.DropTable(
                name: "FeedProviders");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Feeds");
        }
    }
}
