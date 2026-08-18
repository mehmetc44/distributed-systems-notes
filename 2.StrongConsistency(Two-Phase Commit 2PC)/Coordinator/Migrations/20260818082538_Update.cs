using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "NodeStates",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("9e09743e-46bb-475f-8974-cdbaea5ec83f"), "Stock.API" },
                    { new Guid("b8747a37-ca93-441d-a94c-0999bc8f6d0a"), "Payment.API" },
                    { new Guid("ea2c88b8-0896-4137-97f5-2bff282d137b"), "Order.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("9e09743e-46bb-475f-8974-cdbaea5ec83f"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("b8747a37-ca93-441d-a94c-0999bc8f6d0a"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("ea2c88b8-0896-4137-97f5-2bff282d137b"));

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "NodeStates");

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
    }
}
