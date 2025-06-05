using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Be.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccount_RemoveContentPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONTENT_PAGE");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ACCOUNT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ACCOUNT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CONTENT_PAGE",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTENT_PAGE", x => x.PageId);
                    table.ForeignKey(
                        name: "FK_CONTENT_PAGE_ACCOUNT_AccountId",
                        column: x => x.AccountId,
                        principalTable: "ACCOUNT",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONTENT_PAGE_AccountId",
                table: "CONTENT_PAGE",
                column: "AccountId");
        }
    }
}
