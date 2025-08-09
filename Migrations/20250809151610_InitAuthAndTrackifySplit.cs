using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackify.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitAuthAndTrackifySplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preferences_Users_UserId",
                table: "Preferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferenceLinks_Users_UserId",
                table: "UserPreferenceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Users_UserId",
                table: "UserUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_Users_UserId1",
                table: "UserUpdates");

            migrationBuilder.DropIndex(
                name: "IX_UserUpdates_UserId1",
                table: "UserUpdates");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserUpdates");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserUpdates",
                newName: "TrackifyUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserUpdates_UserId",
                table: "UserUpdates",
                newName: "IX_UserUpdates_TrackifyUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserPreferenceLinks",
                newName: "TrackifyUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPreferenceLinks_UserId",
                table: "UserPreferenceLinks",
                newName: "IX_UserPreferenceLinks_TrackifyUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Preferences",
                newName: "TrackifyUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Preferences_UserId",
                table: "Preferences",
                newName: "IX_Preferences_TrackifyUserId");

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RotatedFromId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackifyUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackifyUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackifyUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackifyUsers_UserId",
                table: "TrackifyUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Preferences_TrackifyUsers_TrackifyUserId",
                table: "Preferences",
                column: "TrackifyUserId",
                principalTable: "TrackifyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferenceLinks_TrackifyUsers_TrackifyUserId",
                table: "UserPreferenceLinks",
                column: "TrackifyUserId",
                principalTable: "TrackifyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_TrackifyUsers_TrackifyUserId",
                table: "UserUpdates",
                column: "TrackifyUserId",
                principalTable: "TrackifyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preferences_TrackifyUsers_TrackifyUserId",
                table: "Preferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferenceLinks_TrackifyUsers_TrackifyUserId",
                table: "UserPreferenceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpdates_TrackifyUsers_TrackifyUserId",
                table: "UserUpdates");

            migrationBuilder.DropTable(
                name: "TrackifyUsers");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RotatedFromId",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "TrackifyUserId",
                table: "UserUpdates",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserUpdates_TrackifyUserId",
                table: "UserUpdates",
                newName: "IX_UserUpdates_UserId");

            migrationBuilder.RenameColumn(
                name: "TrackifyUserId",
                table: "UserPreferenceLinks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPreferenceLinks_TrackifyUserId",
                table: "UserPreferenceLinks",
                newName: "IX_UserPreferenceLinks_UserId");

            migrationBuilder.RenameColumn(
                name: "TrackifyUserId",
                table: "Preferences",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Preferences_TrackifyUserId",
                table: "Preferences",
                newName: "IX_Preferences_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserUpdates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserUpdates_UserId1",
                table: "UserUpdates",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Preferences_Users_UserId",
                table: "Preferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferenceLinks_Users_UserId",
                table: "UserPreferenceLinks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_Users_UserId",
                table: "UserUpdates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpdates_Users_UserId1",
                table: "UserUpdates",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
