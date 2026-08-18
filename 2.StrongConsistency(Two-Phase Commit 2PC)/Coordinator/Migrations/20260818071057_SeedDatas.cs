using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class SeedDatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("6ddf110a-2d72-4c78-b4b4-4a4464b8ff8d"), "Stock.API" },
                    { new Guid("8348a39d-d4fb-4a00-af75-05819135b895"), "Order.API" },
                    { new Guid("98c2cb95-c782-4073-afc3-2ead98349519"), "Payment.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("6ddf110a-2d72-4c78-b4b4-4a4464b8ff8d"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("8348a39d-d4fb-4a00-af75-05819135b895"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("98c2cb95-c782-4073-afc3-2ead98349519"));
        }
    }
}
