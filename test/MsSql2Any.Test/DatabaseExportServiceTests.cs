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
            // 创建临时输出目录
            var outputDir = Path.Combine(Path.GetTempPath(), "MsSql2Any_Test_Output_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            try
            {
                // 创建模拟配置
                var mockConfig = new AppConfig
                {
                    SourceConnectionString = "mock_connection_string",
                    OutputDirectory = outputDir, // 使用临时目录
                    BatchSize = 1000
                };

                // 创建模拟数据库提供者
                var mockDbProvider = new Mock<ISourceDbProvider>();

                // 设置模拟方法的返回值
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

                // 模拟数据
                var userData = new List<object[]> { new object[] { 1, "John Doe" }, new object[] { 2, "Jane Smith" } };
                var productData = new List<object[]> { new object[] { 1, "Product A" }, new object[] { 2, "Product B" } };

                mockDbProvider.Setup(x => x.GetDataAsync("Users", 1000))
                    .Returns(userData.ToAsyncEnumerable());

                mockDbProvider.Setup(x => x.GetDataAsync("Products", 1000))
                    .Returns(productData.ToAsyncEnumerable());

                // 创建模拟脚本生成器
                var mockGenerators = new List<IScriptGenerator>
                {
                    new MysqlScriptGenerator(),
                    new SqliteScriptGenerator(),
                    new PostgreSqlScriptGenerator(),
                    new DamengScriptGenerator()
                }.AsEnumerable();

                // 创建服务实例
                var exportService = new DatabaseExportService(mockDbProvider.Object, mockGenerators, mockConfig);

                // 执行导出操作
                await exportService.ExportAsync();

                // 验证模拟对象的方法被调用
                mockDbProvider.Verify(x => x.Initialize("mock_connection_string"), Times.Once);
                mockDbProvider.Verify(x => x.GetTableNamesAsync(), Times.Once);
                // 由于DatabaseExportService为每种数据库类型都会获取表结构，所以GetColumnsAsync会被调用多次
                mockDbProvider.Verify(x => x.GetColumnsAsync("Users"), Times.Exactly(4)); // 4种数据库类型
                mockDbProvider.Verify(x => x.GetColumnsAsync("Products"), Times.Exactly(4)); // 4种数据库类型
                mockDbProvider.Verify(x => x.GetDataAsync("Users", 1000), Times.Exactly(4)); // 4种数据库类型
                mockDbProvider.Verify(x => x.GetDataAsync("Products", 1000), Times.Exactly(4)); // 4种数据库类型

                // 验证输出文件已创建
                var mysqlOutputFile = Path.Combine(outputDir, "output_mysql.sql");
                var sqliteOutputFile = Path.Combine(outputDir, "output_sqlite.sql");
                var postgresqlOutputFile = Path.Combine(outputDir, "output_postgresql.sql");
                var damengOutputFile = Path.Combine(outputDir, "output_dameng.sql");

                Assert.IsTrue(File.Exists(mysqlOutputFile), "MySQL输出文件应存在");
                Assert.IsTrue(File.Exists(sqliteOutputFile), "SQLite输出文件应存在");
                Assert.IsTrue(File.Exists(postgresqlOutputFile), "PostgreSQL输出文件应存在");
                Assert.IsTrue(File.Exists(damengOutputFile), "达梦输出文件应存在");
            }
            finally
            {
                // 清理临时目录
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

    // 辅助扩展方法，用于将列表转换为异步可枚举对象
    public static class TestExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
                await Task.Yield(); // 允许异步操作继续
            }
        }
    }
}