using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Be.Migrations
{
    /// <inheritdoc />
    public partial class Init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "Campaigns",
            //     columns: table => new
            //     {
            //         CampaignId = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //         AccountId = table.Column<int>(type: "int", nullable: false),
            //         PurposeId = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Campaigns", x => x.CampaignId);
            //     });

            migrationBuilder.CreateTable(
                name: "CampaignNgos",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    NgoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignNgos", x => new { x.CampaignId, x.NgoId });
                    table.ForeignKey(
                        name: "FK_CampaignNgos_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignPartners",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPartners", x => new { x.CampaignId, x.PartnerId });
                    table.ForeignKey(
                        name: "FK_CampaignPartners_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignNgos");

            migrationBuilder.DropTable(
                name: "CampaignPartners");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
