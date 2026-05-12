/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;

namespace MsSql2Any.Test
{
    [TestClass]
    public class MysqlScriptGeneratorTests
    {
        [TestMethod]
        public void TestMysqlScriptGeneratorDbType()
        {
            var generator = new MysqlScriptGenerator();
            Assert.AreEqual("MySQL", generator.DbType);
        }

        [TestMethod]
        public void TestMysqlScriptGeneratorCreateTableScript()
        {
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

            StringAssert.Contains(script, "`Id` INT NOT NULL AUTO_INCREMENT");
            StringAssert.Contains(script, "`Name` VARCHAR(50) NOT NULL");
            StringAssert.Contains(script, "`Email` VARCHAR(100)");
            StringAssert.Contains(script, "PRIMARY KEY (`Id`)");
            StringAssert.Contains(script, "CREATE TABLE `Users`");
        }

        [TestMethod]
        public void TestMysqlScriptGeneratorInsertScript()
        {
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

            StringAssert.Contains(script, "INSERT INTO `Users` (`Id`, `Name`) VALUES (1, 'John Doe');");
            StringAssert.Contains(script, "INSERT INTO `Users` (`Id`, `Name`) VALUES (2, 'Jane Smith');");
        }
    }
}