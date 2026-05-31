using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_retries = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_tasks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_job_tasks_created_at",
                table: "job_tasks",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_job_tasks_priority",
                table: "job_tasks",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_job_tasks_status",
                table: "job_tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_job_tasks_status_priority",
                table: "job_tasks",
                columns: new[] { "status", "priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_tasks");
        }
    }
}
