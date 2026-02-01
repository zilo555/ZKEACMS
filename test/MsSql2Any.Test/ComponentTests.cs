using Microsoft.VisualStudio.TestTools.UnitTesting;
using MsSql2Any.Config;
using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;
using MsSql2Any.Services;
using Moq;

namespace MsSql2Any.Test
{
    [TestClass]
    public class ComponentTests
    {
        [TestMethod]
        public void TestAppConfigInitialization()
        {
            // 测试AppConfig类的初始化
            var config = new AppConfig
            {
                SourceConnectionString = "Server=localhost;Database=test;",
                OutputDirectory = "./output",
                BatchSize = 500
            };

            Assert.AreEqual("Server=localhost;Database=test;", config.SourceConnectionString);
            Assert.AreEqual("./output", config.OutputDirectory);
            Assert.AreEqual(500, config.BatchSize);
        }

        [TestMethod]
        public void TestColumnInfoInitialization()
        {
            // 测试ColumnInfo类的初始化
            var columnInfo = new ColumnInfo
            {
                Name = "TestColumn",
                DataType = "varchar",
                IsNullable = true,
                IsPrimaryKey = false,
                IsIdentity = false,
                MaxLength = 50,
                Precision = 10,
                Scale = 2,
                DefaultValue = "'default_value'"
            };

            Assert.AreEqual("TestColumn", columnInfo.Name);
            Assert.AreEqual("varchar", columnInfo.DataType);
            Assert.IsTrue(columnInfo.IsNullable);
            Assert.IsFalse(columnInfo.IsPrimaryKey);
            Assert.IsFalse(columnInfo.IsIdentity);
            Assert.AreEqual(50, columnInfo.MaxLength);
            Assert.AreEqual(10, columnInfo.Precision);
            Assert.AreEqual(2, columnInfo.Scale);
            Assert.AreEqual("'default_value'", columnInfo.DefaultValue);
        }

        [TestMethod]
        public void TestMysqlScriptGeneratorDbType()
        {
            // 测试MySQL脚本生成器的DbType属性
            var generator = new MysqlScriptGenerator();
            Assert.AreEqual("MySQL", generator.DbType);
        }

        [TestMethod]
        public void TestMysqlScriptGeneratorCreateTableScript()
        {
            // 测试MySQL脚本生成器的创建表脚本生成
            var generator = new MysqlScriptGenerator();

            var columns = new List<ColumnInfo>
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
                },
                new ColumnInfo
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    IsNullable = true,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 100
                }
            };

            var script = generator.GenerateCreateTableScript("Users", columns);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "`Id` INT NOT NULL AUTO_INCREMENT");
            StringAssert.Contains(script, "`Name` VARCHAR(50) NOT NULL");
            StringAssert.Contains(script, "`Email` VARCHAR(100)");
            StringAssert.Contains(script, "PRIMARY KEY (`Id`)");
            StringAssert.Contains(script, "CREATE TABLE `Users`");
        }

        [TestMethod]
        public void TestMysqlScriptGeneratorInsertScript()
        {
            // 测试MySQL脚本生成器的插入脚本生成
            var generator = new MysqlScriptGenerator();

            var columns = new List<ColumnInfo>
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

            var dataRows = new List<object[]>
            {
                new object[] { 1, "John Doe" },
                new object[] { 2, "Jane Smith" }
            };

            var script = generator.GenerateInsertScript("Users", columns, dataRows);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "INSERT INTO `Users` (`Id`, `Name`) VALUES (1, 'John Doe');");
            StringAssert.Contains(script, "INSERT INTO `Users` (`Id`, `Name`) VALUES (1, 'John Doe');");
        }

        [TestMethod]
        public void TestSqliteScriptGeneratorDbType()
        {
            // 测试SQLite脚本生成器的DbType属性
            var generator = new SqliteScriptGenerator();
            Assert.AreEqual("SQLite", generator.DbType);
        }

        [TestMethod]
        public void TestSqliteScriptGeneratorCreateTableScript()
        {
            // 测试SQLite脚本生成器的创建表脚本生成
            var generator = new SqliteScriptGenerator();

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = false // SQLite不支持IDENTITY
                },
                new ColumnInfo
                {
                    Name = "Name",
                    DataType = "varchar",
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 50
                },
                new ColumnInfo
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    IsNullable = true,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 100
                }
            };

            var script = generator.GenerateCreateTableScript("Users", columns);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "[Id] INTEGER NOT NULL");
            StringAssert.Contains(script, "[Name] TEXT NOT NULL");
            StringAssert.Contains(script, "[Email] TEXT");
            StringAssert.Contains(script, "PRIMARY KEY ([Id])");
            StringAssert.Contains(script, "CREATE TABLE [Users]");
        }

        [TestMethod]
        public void TestPostgreSqlScriptGeneratorDbType()
        {
            // 测试PostgreSQL脚本生成器的DbType属性
            var generator = new PostgreSqlScriptGenerator();
            Assert.AreEqual("PostgreSQL", generator.DbType);
        }

        [TestMethod]
        public void TestPostgreSqlScriptGeneratorCreateTableScript()
        {
            // 测试PostgreSQL脚本生成器的创建表脚本生成
            var generator = new PostgreSqlScriptGenerator();

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = false // PostgreSQL不直接支持IDENTITY标记
                },
                new ColumnInfo
                {
                    Name = "Name",
                    DataType = "varchar",
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 50
                },
                new ColumnInfo
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    IsNullable = true,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 100
                }
            };

            var script = generator.GenerateCreateTableScript("Users", columns);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "\"Id\" INTEGER NOT NULL");
            StringAssert.Contains(script, "\"Name\" VARCHAR(50) NOT NULL");
            StringAssert.Contains(script, "\"Email\" VARCHAR(100)");
            StringAssert.Contains(script, "CONSTRAINT \"PK_Users\" PRIMARY KEY (\"Id\")");
            StringAssert.Contains(script, "CREATE TABLE \"Users\"");
        }

        [TestMethod]
        public void TestDamengScriptGeneratorDbType()
        {
            // 测试达梦脚本生成器的DbType属性
            var generator = new DamengScriptGenerator();
            Assert.AreEqual("Dameng", generator.DbType);
        }

        [TestMethod]
        public void TestDamengScriptGeneratorCreateTableScript()
        {
            // 测试达梦脚本生成器的创建表脚本生成
            var generator = new DamengScriptGenerator();

            var columns = new List<ColumnInfo>
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
                },
                new ColumnInfo
                {
                    Name = "Email",
                    DataType = "nvarchar",
                    IsNullable = true,
                    IsPrimaryKey = false,
                    IsIdentity = false,
                    MaxLength = 100
                }
            };

            var script = generator.GenerateCreateTableScript("Users", columns);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "\"ID\" INTEGER NOT NULL IDENTITY");
            StringAssert.Contains(script, "\"NAME\" VARCHAR(50) NOT NULL");
            StringAssert.Contains(script, "\"EMAIL\" NVARCHAR(100 char)");
            StringAssert.Contains(script, "PRIMARY KEY (\"ID\")");
            StringAssert.Contains(script, "CREATE TABLE \"USERS\"");
        }

        [TestMethod]
        public void TestSqliteScriptGeneratorInsertScript()
        {
            // 测试SQLite脚本生成器的插入脚本生成
            var generator = new SqliteScriptGenerator();

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = false,
                    MaxLength = 0
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

            var dataRows = new List<object[]>
            {
                new object[] { 1, "John Doe" },
                new object[] { 2, "Jane Smith" }
            };

            var script = generator.GenerateInsertScript("Users", columns, dataRows);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "INSERT INTO [Users] VALUES (1, 'John Doe');");
            StringAssert.Contains(script, "INSERT INTO [Users] VALUES (2, 'Jane Smith');");
        }

        [TestMethod]
        public void TestPostgreSqlScriptGeneratorInsertScript()
        {
            // 测试PostgreSQL脚本生成器的插入脚本生成
            var generator = new PostgreSqlScriptGenerator();

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = false,
                    MaxLength = 0
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

            var dataRows = new List<object[]>
            {
                new object[] { 1, "John Doe" },
                new object[] { 2, "Jane Smith" }
            };

            var script = generator.GenerateInsertScript("Users", columns, dataRows);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "INSERT INTO \"Users\" VALUES (1, 'John Doe');");
            StringAssert.Contains(script, "INSERT INTO \"Users\" VALUES (2, 'Jane Smith');");
        }

        [TestMethod]
        public void TestDamengScriptGeneratorInsertScript()
        {
            // 测试达梦脚本生成器的插入脚本生成
            var generator = new DamengScriptGenerator();

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = false,
                    MaxLength = 0
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

            var dataRows = new List<object[]>
            {
                new object[] { 1, "John Doe" },
                new object[] { 2, "Jane Smith" }
            };

            var script = generator.GenerateInsertScript("Users", columns, dataRows);

            // 验证生成的脚本包含必要的元素
            StringAssert.Contains(script, "INSERT INTO \"USERS\" VALUES (1, 'John Doe');");
            StringAssert.Contains(script, "INSERT INTO \"USERS\" VALUES (2, 'Jane Smith');");
        }
    }
}