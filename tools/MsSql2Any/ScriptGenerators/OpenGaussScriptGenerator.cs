using MsSql2Any.DataAccess;

namespace MsSql2Any.ScriptGenerators;

public class OpenGaussScriptGenerator : IScriptGenerator
{
    public string DbType => "OpenGauss";

    public string GenerateCreateTableScript(string tableName, List<ColumnInfo> columns)
    {
        var script = new System.Text.StringBuilder();
        script.AppendLine($"CREATE TABLE \"{tableName}\" (");

        var columnDefs = new List<string>();
        foreach (var col in columns)
        {
            var columnDef = $"  \"{col.Name}\" {MapDataType(col)}";

            if (!col.IsNullable)
                columnDef += " NOT NULL";

            if (!string.IsNullOrEmpty(col.DefaultValue))
            {
                // Handle default value
                columnDef += $" DEFAULT {col.DefaultValue}";
            }

            columnDefs.Add(columnDef);
        }

        script.Append(string.Join(",\n", columnDefs));

        // Add primary key constraint
        var primaryKeys = columns.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
        if (primaryKeys.Any())
        {
            script.Append($",\n  CONSTRAINT \"PK_{tableName}\" PRIMARY KEY (\"" + string.Join("\", \"", primaryKeys) + "\")");
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

            // For OpenGauss, if there are identity columns, we might want to specify column names
            var identityColumns = columns.Where(c => c.IsIdentity).ToList();
            if (identityColumns.Any())
            {
                var columnNames = string.Join(", ", columns.Select(c => $"\"{c.Name}\""));
                script.AppendLine($"INSERT INTO \"{tableName}\" ({columnNames}) VALUES ({string.Join(", ", values)});");
            }
            else
            {
                script.AppendLine($"INSERT INTO \"{tableName}\" VALUES ({string.Join(", ", values)});");
            }
        }

        return script.ToString();
    }

    private string MapDataType(ColumnInfo column)
    {
        // OpenGauss data type mapping, based on PostgreSQL
        return column.DataType.ToLower() switch
        {
            "int" => "INTEGER",
            "bigint" => "BIGINT",
            "smallint" => "SMALLINT",
            "tinyint" => "SMALLINT", // OpenGauss does not have TINYINT, use SMALLINT
            "bit" => "BOOLEAN", // OpenGauss uses BOOLEAN instead of BIT
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
            "nchar" => $"CHAR({Math.Max(1, column.MaxLength)})", // In OpenGauss, NCHAR is the same as CHAR
            "varchar" => column.MaxLength > 0
                ? $"VARCHAR({column.MaxLength})"
                : "TEXT",
            "nvarchar" => column.MaxLength > 0
                ? $"NVARCHAR({column.MaxLength})" // In OpenGauss, NVARCHAR is the same as VARCHAR
                : "TEXT",
            "text" => "TEXT",
            "ntext" => "TEXT", // In OpenGauss, there is no NTEXT, use TEXT
            "binary" => $"BYTEA", // In OpenGauss, use BYTEA to store binary data
            "varbinary" => $"BYTEA",
            "image" => "BYTEA",
            "uniqueidentifier" => "UUID", // OpenGauss has UUID type
            "xml" => "TEXT", // OpenGauss uses TEXT to store XML
            "timestamp" => "BYTEA", // In OpenGauss, TIMESTAMP is datetime type, not row version
            "rowversion" => "BYTEA", // Use BYTEA to store binary row version data
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
            "binary" or "varbinary" or "image" or "rowversion" or "timestamp" =>
                $"\\x{BitConverter.ToString((byte[])value).Replace("-", "").ToLower()}",
            "bit" => ((bool)value) ? "true" : "false",
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