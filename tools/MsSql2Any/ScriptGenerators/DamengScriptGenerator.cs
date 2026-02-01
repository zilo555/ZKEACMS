using MsSql2Any.DataAccess;

namespace MsSql2Any.ScriptGenerators;

public class DamengScriptGenerator : IScriptGenerator
{
    public string DbType => "Dameng";

    public string GenerateCreateTableScript(string tableName, List<ColumnInfo> columns)
    {
        var script = new System.Text.StringBuilder();
        script.AppendLine($"CREATE TABLE \"{tableName.ToUpper()}\" (");

        var columnDefs = new List<string>();
        foreach (var col in columns)
        {
            var columnDef = $"  \"{col.Name.ToUpper()}\" {MapDataType(col)}";

            if (!col.IsNullable)
                columnDef += " NOT NULL";

            if (!string.IsNullOrEmpty(col.DefaultValue))
            {
                columnDef += $" DEFAULT {col.DefaultValue}";
            }

            if (col.IsIdentity)
                columnDef += " IDENTITY"; // Dameng uses IDENTITY for auto-increment columns

            columnDefs.Add(columnDef);
        }

        script.Append(string.Join(",\n", columnDefs));

        // Add primary key constraint
        var primaryKeys = columns.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
        if (primaryKeys.Any())
        {
            script.Append($",\n  PRIMARY KEY (\"" + string.Join("\", \"", primaryKeys.Select(pk => pk.ToUpper())) + "\")");
        }

        script.AppendLine("\n);");

        return script.ToString();
    }

    public string GenerateInsertScript(string tableName, List<ColumnInfo> columns, IEnumerable<object[]> dataRows)
    {
        var script = new System.Text.StringBuilder();

        // Check if there are identity columns
        var identityColumns = columns.Where(c => c.IsIdentity).ToList();

        if (identityColumns.Any())
        {
            // If there are identity columns, we need to enable IDENTITY_INSERT
            script.AppendLine($"SET IDENTITY_INSERT \"{tableName.ToUpper()}\" ON;");
        }

        foreach (var row in dataRows)
        {
            var values = new List<string>();
            for (int i = 0; i < columns.Count; i++)
            {
                values.Add(FormatValue(row[i], columns[i].DataType));
            }

            // If there are identity columns, we need to specify column names
            if (identityColumns.Any())
            {
                var columnNames = string.Join(", ", columns.Select(c => $"\"{c.Name.ToUpper()}\""));
                script.AppendLine($"INSERT INTO \"{tableName.ToUpper()}\" ({columnNames}) VALUES ({string.Join(", ", values)});");
            }
            else
            {
                script.AppendLine($"INSERT INTO \"{tableName.ToUpper()}\" VALUES ({string.Join(", ", values)});");
            }
        }

        if (identityColumns.Any())
        {
            // If there are identity columns, we need to disable IDENTITY_INSERT
            script.AppendLine($"SET IDENTITY_INSERT \"{tableName.ToUpper()}\" OFF;");
        }

        return script.ToString();
    }

    private string MapDataType(ColumnInfo column)
    {
        // Simplified data type mapping for Dameng database
        return column.DataType.ToLower() switch
        {
            "int" => "INTEGER",
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
            "float" => "DOUBLE PRECISION",
            "real" => "REAL",
            "datetime" => "TIMESTAMP",
            "datetime2" => "TIMESTAMP",
            "smalldatetime" => "TIMESTAMP",
            "date" => "DATE",
            "time" => "TIME",
            "char" => $"CHAR({Math.Max(1, column.MaxLength)})",
            "nchar" => $"CHAR({Math.Max(1, column.MaxLength)})",
            "varchar" => column.MaxLength > 0
                ? $"VARCHAR({column.MaxLength})"
                : "TEXT", 
            "nvarchar" => column.MaxLength > 0
                ? $"NVARCHAR({column.MaxLength} char)"
                : "TEXT",
            "text" => "TEXT",
            "ntext" => "TEXT", // In Dameng, NTEXT is similar to TEXT
            "binary" => $"BINARY({column.MaxLength})",
            "varbinary" => $"VARBINARY({column.MaxLength})",
            "image" => "BLOB",
            "uniqueidentifier" => "CHAR(36)", // In Dameng, use CHAR(36) to store GUID
            "xml" => "CLOB", // In Dameng, use CLOB to store XML
            "timestamp" => "TIMESTAMP", // Note: In Dameng, TIMESTAMP is datetime type, not row version
            "rowversion" => "BLOB", // Use BLOB to store binary row version data
            _ => column.DataType.ToUpper() // Return original type by default
        };
    }

    private string FormatValue(object value, string dataType)
    {
        if (value == DBNull.Value)
            return "NULL";
            
        return dataType.ToLower() switch
        {
            "char" or "nchar" or "varchar" or "nvarchar" or
            "text" or "ntext" or "xml" or "uniqueidentifier" =>
                $"'{EscapeString(value.ToString())}'",
            "datetime" or "datetime2" or "smalldatetime" or "date" or "time" => FormatDateTimeValue(value),
            "binary" or "varbinary" or "image" or "rowversion" =>
                $"0x{BitConverter.ToString((byte[])value).Replace("-", "")}",
            "bit" => ((bool)value) ? "1" : "0",
            _ => value.ToString()
        };
    }

    private string FormatDateTimeValue(object value)
    {
        if (value is DateTime dateTime)
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss.fff}'";
        else
            return $"'{EscapeString(value.ToString())}'";
    }

    private string EscapeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.Replace("'", "''")  // Escape single quotes
                   .Replace("\\", "\\\\"); // Escape backslashes
    }
}