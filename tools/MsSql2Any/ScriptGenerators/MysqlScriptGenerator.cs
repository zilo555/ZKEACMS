/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using MsSql2Any.DataAccess;

namespace MsSql2Any.ScriptGenerators;

public class MysqlScriptGenerator : IScriptGenerator
{
    public string DbType => "MySQL";

    public string GenerateCreateTableScript(string tableName, List<ColumnInfo> columns)
    {
        var script = new System.Text.StringBuilder();
        script.AppendLine($"CREATE TABLE `{tableName}` (");

        var columnDefs = new List<string>();
        foreach (var col in columns)
        {
            var columnDef = $"  `{col.Name}` {MapDataType(col)}";

            if (!col.IsNullable)
                columnDef += " NOT NULL";

            if (!string.IsNullOrEmpty(col.DefaultValue))
            {
                columnDef += $" DEFAULT {col.DefaultValue}";
            }

            if (col.IsIdentity)
                columnDef += " AUTO_INCREMENT";

            columnDefs.Add(columnDef);
        }

        script.Append(string.Join(",\n", columnDefs));

        var primaryKeys = columns.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
        if (primaryKeys.Any())
        {
            script.Append($",\n  PRIMARY KEY (`{string.Join("`, `", primaryKeys)}`)");
        }

        script.AppendLine("\n);");

        return script.ToString();
    }

    public string GenerateInsertScript(string tableName, List<ColumnInfo> columns, IEnumerable<object[]> dataRows)
    {
        var script = new System.Text.StringBuilder();

        foreach (var row in dataRows)
        {
            var values = new List<string>();
            for (int i = 0; i < columns.Count; i++)
            {
                values.Add(FormatValue(row[i], columns[i].DataType));
            }

            // For MySQL, if there are identity/autoincrement columns, we might want to specify column names
            // to avoid issues when inserting explicit values into auto-increment columns
            var identityColumns = columns.Where(c => c.IsIdentity).ToList();
            if (identityColumns.Any())
            {
                var columnNames = string.Join(", ", columns.Select(c => $"`{c.Name}`"));
                script.AppendLine($"INSERT INTO `{tableName}` ({columnNames}) VALUES ({string.Join(", ", values)});");
            }
            else
            {
                script.AppendLine($"INSERT INTO `{tableName}` VALUES ({string.Join(", ", values)});");
            }
        }

        return script.ToString();
    }

    private string MapDataType(ColumnInfo column)
    {
        return column.DataType.ToLower() switch
        {
            "int" => "INT",
            "bigint" => "BIGINT",
            "smallint" => "SMALLINT",
            "tinyint" => "TINYINT",
            "bit" => "BIT",
            "decimal" => column.Scale > 0
                ? $"DECIMAL({column.Precision}, {column.Scale})"
                : $"DECIMAL({column.Precision})",
            "numeric" => column.Scale > 0
                ? $"NUMERIC({column.Precision}, {column.Scale})"
                : $"NUMERIC({column.Precision})",
            "money" => "DECIMAL(19, 4)",
            "smallmoney" => "DECIMAL(10, 4)",
            "float" => "DOUBLE",
            "real" => "FLOAT",
            "datetime" => "DATETIME",
            "datetime2" => "DATETIME",
            "smalldatetime" => "DATETIME",
            "date" => "DATE",
            "time" => "TIME",
            "char" => $"CHAR({Math.Max(1, column.MaxLength)})",
            "nchar" => $"CHAR({Math.Max(1, column.MaxLength)})",
            "varchar" => column.MaxLength > 0
                ? $"VARCHAR({column.MaxLength})"
                : "TEXT",
            "nvarchar" => column.MaxLength > 0
                ? $"VARCHAR({column.MaxLength})" // MySQL does not have NVARCHAR, use VARCHAR
                : "TEXT",
            "text" => "TEXT",
            "ntext" => "LONGTEXT",
            "binary" => $"BINARY({column.MaxLength})",
            "varbinary" => column.MaxLength > 0
                ? $"VARBINARY({column.MaxLength})"
                : "LONGBLOB",
            "image" => "LONGBLOB",
            "uniqueidentifier" => "CHAR(36)", // In MySQL, use CHAR(36) to store UUID
            "xml" => "LONGTEXT",
            "timestamp" => "TIMESTAMP",
            "rowversion" => "BINARY(8)",
            _ => column.DataType.ToUpper() // Return original type by default
        };
    }

    private string FormatValue(object value, string dataType)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return dataType.ToLower() switch
        {
            "char" or "nchar" or "varchar" or "nvarchar" or
            "text" or "ntext" or "xml" or "uniqueidentifier" =>
                $"'{EscapeString(value.ToString())}'",
            "datetime" or "datetime2" or "smalldatetime" or "date" or "time" => FormatDateTimeValue(value)!,
            "binary" or "varbinary" or "image" =>
                $"0x{BitConverter.ToString((byte[])value).Replace("-", "")}",
            "bit" => ((bool)value) ? "1" : "0",
            _ => value.ToString() ?? string.Empty
        };
    }

    private string FormatDateTimeValue(object value)
    {
        if (value is DateTime dateTime)
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss.fff}'";
        else
            return $"'{EscapeString(value?.ToString() ?? string.Empty)}'";
    }

    private string EscapeString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.Replace("\\", "\\\\")
                   .Replace("'", "\\'")
                   .Replace("\0", "\\0")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\"", "\\\"")
                   .Replace("\x1a", "\\Z"); // Ctrl+Z
    }
}