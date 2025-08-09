using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackify.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Release_Entities_EntityId",
                table: "Release");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Release_ReleaseId",
                table: "UserUpdates");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Release",
                table: "Release");

            migrationBuilder.RenameTable(
                name: "Release",
                newName: "Releases");

            migrationBuilder.RenameIndex(
                name: "IX_Release_EntityId",
                table: "Releases",
                newName: "IX_Releases_EntityId");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Releases",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Releases",
                table: "Releases",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preferences_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Preferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPreferenceLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferenceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferenceLinks_Preferences_PreferenceId",
                        column: x => x.PreferenceId,
                        principalTable: "Preferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferenceLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_EntityId",
                table: "Preferences",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_UserId",
                table: "Preferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferenceLinks_PreferenceId",
                table: "UserPreferenceLinks",
                column: "PreferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferenceLinks_UserId",
                table: "UserPreferenceLinks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Entities_EntityId",
                table: "Releases",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_Releases_ReleaseId",
                table: "UserUpdates",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_Entities_EntityId",
                table: "Releases");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Releases_ReleaseId",
                table: "UserUpdates");

            migrationBuilder.DropTable(
                name: "UserPreferenceLinks");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Releases",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Releases");

            migrationBuilder.RenameTable(
                name: "Releases",
                newName: "Release");

            migrationBuilder.RenameIndex(
                name: "IX_Releases_EntityId",
                table: "Release",
                newName: "IX_Release_EntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Release",
                table: "Release",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_EntityId",
                table: "UserPreferences",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId1",
                table: "UserPreferences",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Release_Entities_EntityId",
                table: "Release",
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
    }
}
