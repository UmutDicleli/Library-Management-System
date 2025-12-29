using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLibrary.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberLastName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MemberEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MemberPhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YayinEvis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YayinEviName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YayinEvis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KitapAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookPublishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookFK = table.Column<int>(type: "int", nullable: false),
                    YayinEviFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPublishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookPublishes_Books_BookFK",
                        column: x => x.BookFK,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookPublishes_YayinEvis_YayinEviFK",
                        column: x => x.YayinEviFK,
                        principalTable: "YayinEvis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentBooks",
                columns: table => new
                {
                    BookPublishFK = table.Column<int>(type: "int", nullable: false),
                    MemberFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentBooks", x => x.BookPublishFK);
                    table.ForeignKey(
                        name: "FK_RentBooks_BookPublishes_BookPublishFK",
                        column: x => x.BookPublishFK,
                        principalTable: "BookPublishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentBooks_Members_MemberFK",
                        column: x => x.MemberFK,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_AuthorName",
                table: "Authors",
                column: "AuthorName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookPublishes_BookFK",
                table: "BookPublishes",
                column: "BookFK");

            migrationBuilder.CreateIndex(
                name: "IX_BookPublishes_YayinEviFK",
                table: "BookPublishes",
                column: "YayinEviFK");

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorId",
                table: "Books",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberEmail",
                table: "Members",
                column: "MemberEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberLastName",
                table: "Members",
                column: "MemberLastName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberPhoneNumber",
                table: "Members",
                column: "MemberPhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentBooks_MemberFK",
                table: "RentBooks",
                column: "MemberFK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentBooks");

            migrationBuilder.DropTable(
                name: "BookPublishes");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "YayinEvis");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
