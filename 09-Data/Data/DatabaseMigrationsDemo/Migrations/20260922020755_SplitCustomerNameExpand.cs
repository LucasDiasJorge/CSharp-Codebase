using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseMigrationsDemo.Migrations
{
    /// <inheritdoc />
    public partial class SplitCustomerNameExpand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { null, null });

            // BACKFILL — a parte que o EF nao consegue gerar sozinho.
            //
            // Scaffolding produz a mudanca de SCHEMA a partir do diff do modelo; mover
            // DADOS de uma coluna para outra e decisao de negocio, e precisa ser escrita
            // a mao. Sem isto, as colunas novas nasceriam vazias e a etapa "expand"
            // estaria incompleta.
            migrationBuilder.Sql(@"
                UPDATE Customers
                SET FirstName = CASE
                        WHEN instr(Name, ' ') > 0 THEN substr(Name, 1, instr(Name, ' ') - 1)
                        ELSE Name
                    END,
                    LastName = CASE
                        WHEN instr(Name, ' ') > 0 THEN substr(Name, instr(Name, ' ') + 1)
                        ELSE ''
                    END
                WHERE FirstName IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Customers");
        }
    }
}
