# MsSql2Any 工具技术架构文档

## 1. 核心功能
1. 读取配置文件或命令行参数，获取 SQL Server 源数据库连接字符串。
2. 连接 SQL Server 数据库。
3. 提取数据库中的表结构（包括列定义、主键、索引等，暂不处理外键约束，因为跨数据库的外键重建可能更复杂且通常在应用层面保证）。
4. 提取每个表的数据。
5. 将提取的表结构和数据转换为所有支持的目标数据库（MySQL、SQLite、PostgreSQL、Dameng）的 SQL 脚本。
6. 将每种目标数据库的脚本分别输出到不同的文件中。

## 2. 架构组件
1. **入口点 (`Program.cs`)**
   - 启动应用程序。
   - 初始化配置系统（`IConfiguration`）。
   - 通过依赖注入容器（可选，但推荐用于管理多个服务）解析并注入核心服务。
   - 启动主导出流程。

2. **配置管理**
   - 使用 .NET Core 的 `ConfigurationBuilder`。
   - 支持多种配置源：JSON 文件（如 `appsettings.json`）、环境变量、命令行参数。
   - 主要配置项：
     - `SourceConnectionString`: SQL Server 源数据库连接字符串。
     - `OutputDirectory`: 输出脚本文件的目录（可选，默认当前目录）。
     - `BatchSize`: 读取数据时的批次大小（可选，默认值）。

3. **数据库访问层 (DAL)**
   - **`ISourceDbProvider`**: 定义从源数据库（SQL Server）读取元数据和数据的接口。
   - **`SqlServerProvider`**: 实现 `ISourceDbProvider`，使用 `Microsoft.Data.SqlClient` 连接 SQL Server。
   - **元数据提取**: 获取表列表、列信息（名称、类型、长度、是否可空、默认值、主键、索引）。
   - **数据提取**: 根据配置的 `BatchSize` 分批读取表数据。

4. **脚本生成器层**
   - **`IScriptGenerator`**: 定义生成目标数据库脚本的接口（如 `GenerateCreateTableScript`, `GenerateInsertScript`）。
   - **具体实现**:
     - `MysqlScriptGenerator`
     - `SqliteScriptGenerator`
     - `PostgreSqlScriptGenerator`
     - `DamengScriptGenerator`
   - 每个实现类负责处理特定数据库的语法差异，如数据类型映射、标识列、字符串转义、关键字等。

5. **数据类型映射**
   - 在各个 `IScriptGenerator` 实现内部，维护 SQL Server 类型到目标数据库类型的映射逻辑。

6. **主控制器/服务**
   - **`DatabaseExportService`**: 协调整个导出过程。
     - 从 `ISourceDbProvider` 获取表结构和数据。
     - 遍历所有注册的 `IScriptGenerator` 实例。
     - 为每个目标数据库生成完整的脚本（包含所有表的创建和数据插入语句）。
     - 将生成的脚本写入对应的输出文件（例如 `output_mysql.sql`, `output_sqlite.sql` 等）。

## 3. 关键考虑点
1. **性能**: 分批读取数据，避免加载大量数据到内存。
2. **错误处理**: 处理数据库连接、查询、文件写入等环节可能出现的异常。
3. **扩展性**: 通过接口 `IScriptGenerator` 易于添加新的目标数据库。
4. **SQL 安全**: 在生成 `INSERT` 语句时，正确转义字符串和特殊字符。
5. **依赖项**:
   - `Microsoft.Data.SqlClient`
   - `Microsoft.Extensions.Configuration.Binder`
   - `Microsoft.Extensions.Hosting` (可选，用于 DI 和 Hosted Services)
