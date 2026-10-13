using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace urlshortenerapi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncludeIpHashInClickIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClickEvents_ShortLinkId_OccurredAt",
                table: "ClickEvents");

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortLinkId_OccurredAt",
                table: "ClickEvents",
                columns: new[] { "ShortLinkId", "OccurredAt" })
                .Annotation("SqlServer:Include", new[] { "RefererHost", "IpHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClickEvents_ShortLinkId_OccurredAt",
                table: "ClickEvents");

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortLinkId_OccurredAt",
                table: "ClickEvents",
                columns: new[] { "ShortLinkId", "OccurredAt" })
                .Annotation("SqlServer:Include", new[] { "RefererHost" });
        }
    }
}
