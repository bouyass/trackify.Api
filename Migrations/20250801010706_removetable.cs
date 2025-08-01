using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackify.Api.Migrations
{
    /// <inheritdoc />
    public partial class removetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_Contents_ContentId",
                table: "UserPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Updates_UpdateLogId",
                table: "UserUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Updates_UpdateLogId1",
                table: "UserUpdates");

            migrationBuilder.DropTable(
                name: "ExternalProviders");

            migrationBuilder.DropTable(
                name: "Updates");

            migrationBuilder.DropTable(
                name: "Contents");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_UserUpdates_UpdateLogId1",
                table: "UserUpdates");

            migrationBuilder.DropColumn(
                name: "UpdateLogId1",
                table: "UserUpdates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserPreferences");

            migrationBuilder.RenameColumn(
                name: "UpdateLogId",
                table: "UserUpdates",
                newName: "ReleaseId");

            migrationBuilder.RenameIndex(
                name: "IX_UserUpdates_UpdateLogId",
                table: "UserUpdates",
                newName: "IX_UserUpdates_ReleaseId");

            migrationBuilder.RenameColumn(
                name: "ContentId",
                table: "UserPreferences",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPreferences_ContentId",
                table: "UserPreferences",
                newName: "IX_UserPreferences_EntityId");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "UserUpdates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    AuthorOrArtist = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Release",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Release", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Release_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Release_EntityId",
                table: "Release",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Entities_EntityId",
                table: "UserPreferences",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_Release_ReleaseId",
                table: "UserUpdates",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_Entities_EntityId",
                table: "UserPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Release_ReleaseId",
                table: "UserUpdates");

            migrationBuilder.DropTable(
                name: "Release");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserUpdates");

            migrationBuilder.RenameColumn(
                name: "ReleaseId",
                table: "UserUpdates",
                newName: "UpdateLogId");

            migrationBuilder.RenameIndex(
                name: "IX_UserUpdates_ReleaseId",
                table: "UserUpdates",
                newName: "IX_UserUpdates_UpdateLogId");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "UserPreferences",
                newName: "ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPreferences_EntityId",
                table: "UserPreferences",
                newName: "IX_UserPreferences_ContentId");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateLogId1",
                table: "UserUpdates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ProviderUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalProviders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contents_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Updates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Updates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Updates_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Updates_Contents_ContentId1",
                        column: x => x.ContentId1,
                        principalTable: "Contents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserUpdates_UpdateLogId1",
                table: "UserUpdates",
                column: "UpdateLogId1");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CategoryId",
                table: "Contents",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalProviders_UserId",
                table: "ExternalProviders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_ContentId",
                table: "Updates",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_ContentId1",
                table: "Updates",
                column: "ContentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Contents_ContentId",
                table: "UserPreferences",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_Updates_UpdateLogId",
                table: "UserUpdates",
                column: "UpdateLogId",
                principalTable: "Updates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_Updates_UpdateLogId1",
                table: "UserUpdates",
                column: "UpdateLogId1",
                principalTable: "Updates",
                principalColumn: "Id");
        }
    }
}
