using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenArcade.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comment_likes",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment_likes", x => new { x.user_id, x.comment_id });
                });

            migrationBuilder.CreateTable(
                name: "game_checks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    result = table.Column<string>(type: "text", nullable: false),
                    screenshot_url = table.Column<string>(type: "text", nullable: true),
                    checked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_checks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plays",
                columns: table => new
                {
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visitor_id = table.Column<string>(type: "text", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plays", x => new { x.game_id, x.visitor_id, x.day });
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    handled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    keycloak_id = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "votes",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", maxLength: 32, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_votes", x => new { x.user_id, x.game_id });
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    like_count = table.Column<int>(type: "integer", nullable: false),
                    pinned = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "publisher_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    studio_name = table.Column<string>(type: "text", nullable: false),
                    links = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publisher_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_publisher_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "publishers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    studio_name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publishers", x => x.id);
                    table.ForeignKey(
                        name: "fk_publishers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    play_url = table.Column<string>(type: "text", nullable: false),
                    domain = table.Column<string>(type: "text", nullable: false),
                    verification_token = table.Column<string>(type: "text", nullable: false),
                    domain_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    mobile_friendly = table.Column<bool>(type: "boolean", nullable: false),
                    consecutive_health_failures = table.Column<int>(type: "integer", nullable: false),
                    like_count = table.Column<int>(type: "integer", nullable: false),
                    dislike_count = table.Column<int>(type: "integer", nullable: false),
                    play_count = table.Column<int>(type: "integer", nullable: false),
                    play_count30d = table.Column<int>(type: "integer", nullable: false),
                    trending_score = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_games", x => x.id);
                    table.ForeignKey(
                        name: "fk_games_publishers_publisher_id",
                        column: x => x.publisher_id,
                        principalTable: "publishers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorites", x => new { x.user_id, x.game_id });
                    table.ForeignKey(
                        name: "fk_favorites_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_media_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comments_game_id_parent_id_created_at",
                table: "comments",
                columns: new[] { "game_id", "parent_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id",
                table: "comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_game_id",
                table: "favorites",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_checks_game_id_kind_checked_at",
                table: "game_checks",
                columns: new[] { "game_id", "kind", "checked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_game_media_game_id",
                table: "game_media",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_games_publisher_id",
                table: "games",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_games_slug",
                table: "games",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_games_status",
                table: "games",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_games_tags",
                table: "games",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_plays_game_id_day",
                table: "plays",
                columns: new[] { "game_id", "day" });

            migrationBuilder.CreateIndex(
                name: "ix_publisher_requests_user_id",
                table: "publisher_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_publishers_user_id",
                table: "publishers",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reports_status_created_at",
                table: "reports",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_keycloak_id",
                table: "users",
                column: "keycloak_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_votes_game_id_updated_at",
                table: "votes",
                columns: new[] { "game_id", "updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comment_likes");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "game_checks");

            migrationBuilder.DropTable(
                name: "game_media");

            migrationBuilder.DropTable(
                name: "plays");

            migrationBuilder.DropTable(
                name: "publisher_requests");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "votes");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "publishers");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
