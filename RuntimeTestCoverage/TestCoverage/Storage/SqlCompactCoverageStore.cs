using Dapper;
using ErikEJ.SqlCe;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;

namespace TestCoverage.Storage
{
    public class SqlCompactCoverageStore : ICoverageStore
    {
        private readonly string _filePath;


        public SqlCompactCoverageStore()
        {
            _filePath = Path.Combine(Config.WorkingDirectory, "CoverageCache.coverage");
        }

        public void AppendByDocumentPath(string documentPath, IEnumerable<LineCoverage> coverage)
        {
            using (var connection = new SqlCeConnection(GetConnectionString()))
            {
                connection.Open();

                const string delete =
                "DELETE FROM Coverage where DocumentPath=@documentPath or TestDocumentPath=@documentPath";

                connection.Execute(delete, new { documentPath });
                InsertLineCoverage(connection, coverage.ToArray());
            }
        }

        public void Append( IEnumerable<LineCoverage> coverage)
        {
            string[] testMethods = coverage.Select(x => x.TestPath).Distinct().ToArray();

            if (testMethods.Length == 0)
                return;

            using (var connection = new SqlCeConnection(GetConnectionString()))
            {
                connection.Open();

                const string delete =
                "DELETE FROM Coverage where TestPath in @testMethods";

                connection.Execute(delete, new { testMethods });
                InsertLineCoverage(connection, coverage.ToArray());
            }
        }

        public void WriteAll(IEnumerable<LineCoverage> coverage)
        {
            SetupDatabase();

            using (var connection = new SqlCeConnection(GetConnectionString()))
            {
                connection.Open();
                InsertLineCoverage(connection, coverage.ToArray());
            }
        }

        public LineCoverage[] ReadAll()
        {
            if (!File.Exists(_filePath))
                return new LineCoverage[0];

            using (var connection = new SqlCeConnection(GetConnectionString()))
            {
                var data = connection.Query<LineCoverage>("SELECT * FROM Coverage");

                return data.ToArray();
            }
        }


        private void SetupDatabase()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            using (var engine = new SqlCeEngine(GetConnectionString()))
            {
                engine.CreateDatabase();
            }

            SetupTable();
        }

        private void InsertLineCoverage(SqlCeConnection connection, LineCoverage[] coverage)
        {
            using (DataTable table = new DataTable())
            {
                table.Columns.Add("NodePath");
                table.Columns.Add("TestPath");
                table.Columns.Add("DocumentPath");
                table.Columns.Add("TestDocumentPath");
                table.Columns.Add("Span");
                table.Columns.Add("IsSuccess");
                table.Columns.Add("ErrorMessage");

                foreach (var lineCoverage in coverage)
                {
                    DataRow row = table.NewRow();
                    row["NodePath"] = lineCoverage.NodePath;
                    row["TestPath"] = lineCoverage.TestPath;
                    row["DocumentPath"] = lineCoverage.DocumentPath;
                    row["TestDocumentPath"] = lineCoverage.TestDocumentPath;
                    row["Span"] = lineCoverage.Span;
                    row["IsSuccess"] = lineCoverage.IsSuccess;
                    row["ErrorMessage"] = lineCoverage.ErrorMessage.Truncate(4000);

                    table.Rows.Add(row);
                }

                DoBulkCopy(connection, new DataTableReader(table));
            }
        }


        private void DoBulkCopy(SqlCeConnection connection, IDataReader reader)
        {
            SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();

            using (SqlCeBulkCopy bc = new SqlCeBulkCopy(connection, options))
            {
                bc.DestinationTableName = "Coverage";
                bc.WriteToServer(reader);
            }
        }

        private void SetupTable()
        {
            using (var connection = new SqlCeConnection(GetConnectionString()))
            {
                var sql = "create table Coverage("
                          + "NodePath nvarchar(500) not null, "
                          + "TestPath nvarchar(500) not null, "
                          + "ErrorMessage nvarchar(4000) null, "
                          + "DocumentPath nvarchar(500)  not null, "
                          + "TestDocumentPath nvarchar(500) not null, "
                          + "Span int not null, "
                          + "IsSuccess bit not null)";

                connection.Execute(sql);
            }
        }


        private string GetConnectionString()
        {
            return $"Data Source={_filePath};Persist Security Info=False;";
        }
    }
}