using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlfaBank.Core.Migrations
{
    /// <summary>
    /// Начальная миграция схемы базы данных проекта.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AnnualInterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinTermMonths = table.Column<int>(type: "int", nullable: false),
                    MaxTermMonths = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RegistrationAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EmployerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EmploymentMonths = table.Column<int>(type: "int", nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyExpenses = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientProfileId = table.Column<int>(type: "int", nullable: false),
                    CreditProductId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    DecisionComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ScorePoints = table.Column<int>(type: "int", nullable: true),
                    PaymentSharePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplications_ClientProfiles_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditApplications_CreditProducts_CreditProductId",
                        column: x => x.CreditProductId,
                        principalTable: "CreditProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditApplications_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditApplicationId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStatusHistory_CreditApplications_CreditApplicationId",
                        column: x => x.CreditApplicationId,
                        principalTable: "CreditApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationStatusHistory_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Credits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditApplicationId = table.Column<int>(type: "int", nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualInterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClosedOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credits_CreditApplications_CreditApplicationId",
                        column: x => x.CreditApplicationId,
                        principalTable: "CreditApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditScheduleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingDebt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaidOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditScheduleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditScheduleItems_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditId = table.Column<int>(type: "int", nullable: false),
                    CreditScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    PaidOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegisteredByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_CreditScheduleItems_CreditScheduleItemId",
                        column: x => x.CreditScheduleItemId,
                        principalTable: "CreditScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_RegisteredByUserId",
                        column: x => x.RegisteredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CreditProducts",
                columns: new[] { "Id", "AnnualInterestRate", "CreatedAt", "Description", "IsActive", "MaxAmount", "MaxTermMonths", "MinAmount", "MinTermMonths", "Name" },
                values: new object[,]
                {
                    { 1, 18.5m, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Кредит наличными на любые нужды без залога и поручителей.", true, 1500000m, 60, 50000m, 6, "Потребительский кредит" },
                    { 2, 16.0m, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Объединение нескольких кредитов в один по сниженной ставке.", true, 3000000m, 84, 100000m, 12, "Рефинансирование" },
                    { 3, 22.0m, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Оборотные средства для малого и среднего бизнеса.", true, 5000000m, 36, 200000m, 6, "Кредит для предпринимателей" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "Login", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Соколова Ирина Петровна", true, "specialist", "PBKDF2-SHA256$210000$66LaxU3dH0LzWUowf+15zQ==$XSCjJo2HzBDOX93jvt79r/CgRtCYFVLss52ttpsnCKc=", 2 },
                    { 2, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Ковалёв Артём Игоревич", true, "administrator", "PBKDF2-SHA256$210000$LRfRnYHrBGfkj+TgEUqiwA==$z2PJtAbx9TX+7hoZu13WFVHqoVVkTEe1QkzZmwmniZk=", 3 },
                    { 3, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Иванов Иван Иванович", true, "client", "PBKDF2-SHA256$210000$Y6SL/xSlIgIdQAWKWoIrMQ==$XL8oh5+Xzvp6DZaFa9UGzg0/2D/Z+tvM5vXyu3qdOp4=", 1 },
                    { 4, new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Петрова Анна Сергеевна", true, "client2", "PBKDF2-SHA256$210000$YYHFp2WHnQA7lebRkA7u7w==$LcqxYkyAVlWb7SHbiBUgTmJnQfYOF7VjfJS5+gXCA7M=", 1 }
                });

            migrationBuilder.InsertData(
                table: "ClientProfiles",
                columns: new[] { "Id", "BirthDate", "EmployerName", "EmploymentMonths", "MonthlyExpenses", "MonthlyIncome", "PassportNumber", "RegistrationAddress", "UserId" },
                values: new object[,]
                {
                    { 1, new DateOnly(1990, 4, 15), "ООО «Ромашка»", 84, 45000m, 120000m, "45 12 345678", "г. Москва, ул. Тверская, д. 1, кв. 5", 3 },
                    { 2, new DateOnly(1996, 11, 2), "АО «Вектор»", 30, 30000m, 85000m, "52 98 765432", "г. Санкт-Петербург, пр-т Невский, д. 20, кв. 44", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistory_ChangedByUserId",
                table: "ApplicationStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistory_CreditApplicationId",
                table: "ApplicationStatusHistory",
                column: "CreditApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfiles_UserId",
                table: "ClientProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_ClientProfileId",
                table: "CreditApplications",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_CreditProductId",
                table: "CreditApplications",
                column: "CreditProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_ReviewedByUserId",
                table: "CreditApplications",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_Status_CreatedAt",
                table: "CreditApplications",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditScheduleItems_CreditId_Number",
                table: "CreditScheduleItems",
                columns: new[] { "CreditId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credits_CreditApplicationId",
                table: "Credits",
                column: "CreditApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreditId",
                table: "Payments",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreditScheduleItemId",
                table: "Payments",
                column: "CreditScheduleItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RegisteredByUserId",
                table: "Payments",
                column: "RegisteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationStatusHistory");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "CreditScheduleItems");

            migrationBuilder.DropTable(
                name: "Credits");

            migrationBuilder.DropTable(
                name: "CreditApplications");

            migrationBuilder.DropTable(
                name: "ClientProfiles");

            migrationBuilder.DropTable(
                name: "CreditProducts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
