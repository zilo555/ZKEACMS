using Microsoft.VisualStudio.TestTools.UnitTesting;
using MsSql2Any.Config;
using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;
using MsSql2Any.Services;
using Moq;

namespace MsSql2Any.Test
{
    [TestClass]
    public class DatabaseExportServiceTests
    {
        [TestMethod]
        public async Task TestDatabaseExportServiceWithMockData()
        {
            // Create temporary output directory
            var outputDir = Path.Combine(Path.GetTempPath(), "MsSql2Any_Test_Output_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            try
            {
                // Create mock configuration
                var mockConfig = new AppConfig
                {
                    SourceConnectionString = "mock_connection_string",
                    OutputDirectory = outputDir, // Using temporary directory
                    BatchSize = 1000
                };

                // Create mock database provider
                var mockDbProvider = new Mock<ISourceDbProvider>();

                // Setup mock method return values
                var tableNames = new List<string> { "Users", "Products" };
                mockDbProvider.Setup(x => x.GetTableNamesAsync()).ReturnsAsync(tableNames);

                var userColumns = new List<ColumnInfo>
                {
                    new ColumnInfo
                    {
                        Name = "Id",
                        DataType = "int",
                        IsNullable = false,
                        IsPrimaryKey = true,
                        IsIdentity = true
                    },
                    new ColumnInfo
                    {
                        Name = "Name",
                        DataType = "varchar",
                        IsNullable = false,
                        IsPrimaryKey = false,
                        IsIdentity = false,
                        MaxLength = 50
                    }
                };

                var productColumns = new List<ColumnInfo>
                {
                    new ColumnInfo
                    {
                        Name = "Id",
                        DataType = "int",
                        IsNullable = false,
                        IsPrimaryKey = true,
                        IsIdentity = true
                    },
                    new ColumnInfo
                    {
                        Name = "ProductName",
                        DataType = "nvarchar",
                        IsNullable = false,
                        IsPrimaryKey = false,
                        IsIdentity = false,
                        MaxLength = 100
                    }
                };

                mockDbProvider.Setup(x => x.GetColumnsAsync("Users")).ReturnsAsync(userColumns);
                mockDbProvider.Setup(x => x.GetColumnsAsync("Products")).ReturnsAsync(productColumns);

                // Mock data
                var userData = new List<object[]> { new object[] { 1, "John Doe" }, new object[] { 2, "Jane Smith" } };
                var productData = new List<object[]> { new object[] { 1, "Product A" }, new object[] { 2, "Product B" } };

                mockDbProvider.Setup(x => x.GetDataAsync("Users", 1000))
                    .Returns(userData.ToAsyncEnumerable());

                mockDbProvider.Setup(x => x.GetDataAsync("Products", 1000))
                    .Returns(productData.ToAsyncEnumerable());

                // Create mock script generators
                var mockGenerators = new List<IScriptGenerator>
                {
                    new MysqlScriptGenerator(),
                    new SqliteScriptGenerator(),
                    new PostgreSqlScriptGenerator(),
                    new DamengScriptGenerator()
                }.AsEnumerable();

                // Create service instance
                var exportService = new DatabaseExportService(mockDbProvider.Object, mockGenerators, mockConfig);

                // Execute export operation
                await exportService.ExportAsync();

                // Verify mock object methods were called
                mockDbProvider.Verify(x => x.Initialize("mock_connection_string"), Times.Once);
                mockDbProvider.Verify(x => x.GetTableNamesAsync(), Times.Once);
                // Since DatabaseExportService retrieves table structure for each database type, GetColumnsAsync is called multiple times
                mockDbProvider.Verify(x => x.GetColumnsAsync("Users"), Times.Exactly(4)); // 4 database types
                mockDbProvider.Verify(x => x.GetColumnsAsync("Products"), Times.Exactly(4)); // 4 database types
                mockDbProvider.Verify(x => x.GetDataAsync("Users", 1000), Times.Exactly(4)); // 4 database types
                mockDbProvider.Verify(x => x.GetDataAsync("Products", 1000), Times.Exactly(4)); // 4 database types

                // Verify output files were created
                var mysqlOutputFile = Path.Combine(outputDir, "MySQL.sql");
                var sqliteOutputFile = Path.Combine(outputDir, "SQLite.sql");
                var postgresqlOutputFile = Path.Combine(outputDir, "PostgreSQL.sql");
                var damengOutputFile = Path.Combine(outputDir, "Dameng.sql");

                Assert.IsTrue(File.Exists(mysqlOutputFile), "MySQL output file should exist");
                Assert.IsTrue(File.Exists(sqliteOutputFile), "SQLite output file should exist");
                Assert.IsTrue(File.Exists(postgresqlOutputFile), "PostgreSQL output file should exist");
                Assert.IsTrue(File.Exists(damengOutputFile), "Dameng output file should exist");
            }
            finally
            {
                // Clean up temporary directory
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }
            }
        }

        [TestMethod]
        public void TestAllScriptGeneratorsHaveCorrectDbType()
        {
            var generators = new List<IScriptGenerator>
            {
                new MysqlScriptGenerator(),
                new SqliteScriptGenerator(),
                new PostgreSqlScriptGenerator(),
                new DamengScriptGenerator()
            };

            Assert.AreEqual("MySQL", generators[0].DbType);
            Assert.AreEqual("SQLite", generators[1].DbType);
            Assert.AreEqual("PostgreSQL", generators[2].DbType);
            Assert.AreEqual("Dameng", generators[3].DbType);
        }

        [TestMethod]
        public void TestDataTypeMappingForAllGenerators()
        {
            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = true,
                    MaxLength = 0,
                    Precision = 0,
                    Scale = 0,
                    DefaultValue = ""
                },
                new ColumnInfo
                {
                    Name = "Name",
                    DataType = "varchar",
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 50,
                    Precision = 0,
                    Scale = 0,
                    DefaultValue = ""
                },
                new ColumnInfo
                {
                    Name = "Price",
                    DataType = "decimal",
                    IsNullable = true,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 0,
                    Precision = 10,
                    Scale = 2,
                    DefaultValue = ""
                }
            };

            var generators = new List<IScriptGenerator>
            {
                new MysqlScriptGenerator(),
                new SqliteScriptGenerator(),
                new PostgreSqlScriptGenerator(),
                new DamengScriptGenerator()
            };

            foreach (var generator in generators)
            {
                var createScript = generator.GenerateCreateTableScript("TestTable", columns);
                if (generator is DamengScriptGenerator)
                {
                    Assert.Contains("TestTable".ToUpper(), createScript);
                    Assert.Contains(columns[0].Name.ToUpper(), createScript);
                    Assert.Contains(columns[1].Name.ToUpper(), createScript);
                    Assert.Contains(columns[2].Name.ToUpper(), createScript);
                }
                else
                {
                    Assert.Contains("TestTable", createScript);
                    Assert.Contains(columns[0].Name, createScript);
                    Assert.Contains(columns[1].Name, createScript);
                    Assert.Contains(columns[2].Name, createScript);
                }
                
            }
        }
    }

    // Helper extension method to convert a list to an asynchronous enumerable
    public static class TestExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
                await Task.Yield(); // Allow async operation to continue
            }
        }
    }
}