# ZKEACMS 数据库
## Microsoft Sql Server
使用 Build.cmd 来创建数据库

![create](https://user-images.githubusercontent.com/6006218/30580006-a9c9507c-9d4d-11e7-9fcc-8a3eb40e2ffd.gif)

或者使用`script.sql`脚本来创建数据库

## MySql
使用 MySql 目录下对应的Dump脚本来创建数据库。

## SQLite
使用 SQLite 目录下的 [Build.cmd](SQLite/Build.cmd) 初始化数据库。

初始化前需要先安装 `sqlite-exec` 工具：

```powershell
dotnet tool install --global sqlite-exec
```

执行后会在当前工作目录生成 `Database.sqlite`。

如果需要导出数据库脚本，请使用根目录下的 [Export.cmd](Export.cmd)。该脚本会根据输入的 SQL Server 连接信息同时生成：

- [SQLite/ZKEACMS.sqlite.sql](SQLite/ZKEACMS.sqlite.sql)
- [MySql/Dump.sql](MySql/Dump.sql)
- 根目录下的 [script.sql](script.sql)

## 导出脚本说明

导出脚本需要以下环境依赖：

- [UV](https://docs.astral.sh/uv/)
- 由 UV 管理的 Python 环境
- `mssql-scripter` 和 `mssql2mysql` 可通过 UV 在脚本中调用
- SQLite 目录下的 [Export2SQLCE.exe](SQLite/Export2SQLCE.exe)

使用前请确认：

1. 已存在可访问的 SQL Server 实例。
2. 已安装 UV，并且可正常执行 `uv` 命令。
3. UV 的 Python 环境已初始化。
4. `Export2SQLCE.exe` 位于 [SQLite](SQLite) 目录中。
5. 脚本中的服务器地址、数据库名、用户名和密码已按本机环境调整。

如果还没有初始化 UV 环境，可以在 [Database](.) 目录下执行下面的命令：

```powershell
uv venv
uv pip install mssql-scripter mssql2mysql
```

如果你已经有现成的 `.venv`，也可以直接激活后再运行 [Export.cmd](Export.cmd)。

