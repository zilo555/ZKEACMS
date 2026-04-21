/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;

namespace MsSql2Any.Test
{
    [TestClass]
    public class PostgreSqlScriptGeneratorTests
    {
        [TestMethod]
        public void TestPostgreSqlScriptGeneratorDbType()
        {
            var generator = new PostgreSqlScriptGenerator();
            Assert.AreEqual("PostgreSQL", generator.DbType);
        }

        [TestMethod]
        public void TestPostgreSqlScriptGeneratorCreateTableScript()
        {
            var generator = new PostgreSqlScriptGenerator();

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "Id",
                    DataType = "int",
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsIdentity = false
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

            StringAssert.Contains(script, "\"Id\" INTEGER NOT NULL");
            StringAssert.Contains(script, "\"Name\" VARCHAR(50) NOT NULL");
            StringAssert.Contains(script, "\"Email\" VARCHAR(100)");
            StringAssert.Contains(script, "CONSTRAINT \"PK_Users\" PRIMARY KEY (\"Id\")");
            StringAssert.Contains(script, "CREATE TABLE \"Users\"");
        }

        [TestMethod]
        public void TestPostgreSqlScriptGeneratorInsertScript()
        {
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

            StringAssert.Contains(script, "INSERT INTO \"Users\" VALUES (1, 'John Doe');");
            StringAssert.Contains(script, "INSERT INTO \"Users\" VALUES (2, 'Jane Smith');");
        }
    }
}