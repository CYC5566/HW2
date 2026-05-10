using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace USUN2.Data.Infrastructure;

public static partial class FinanceDatabaseBootstrap
{
    private static readonly string[] ScriptOrder = ["01_DDL.sql", "02_StoredProcedures.sql"];

    [GeneratedRegex(@"^\s*GO\s*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoBatchRegex();

    public static async Task EnsureReadyAsync(
        string connectionString,
        string sqlScriptsDirectory,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var csb = new SqlConnectionStringBuilder(connectionString);
        var dbName = csb.InitialCatalog;
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("連線字串必須指定 Initial Catalog（資料庫名稱）。");

        ValidateDatabaseName(dbName);

        csb.InitialCatalog = "master";
        await using var connection = new SqlConnection(csb.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await EnsureDatabaseExistsAsync(connection, dbName, cancellationToken);
        connection.ChangeDatabase(dbName);

        if (await SchemaIsCurrentAsync(connection, cancellationToken))
        {
            logger.LogDebug("資料庫 {Database} 結構已為最新，略過腳本套用。", dbName);
            return;
        }

        logger.LogInformation("正在初始化資料庫 {Database}（套用 {Dir}）…", dbName, sqlScriptsDirectory);

        foreach (var fileName in ScriptOrder)
        {
            var path = Path.Combine(sqlScriptsDirectory, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"找不到 SQL 腳本：{path}");

            var script = await File.ReadAllTextAsync(path, cancellationToken);
            foreach (var batch in SplitSqlBatches(script))
            {
                await using var cmd = new SqlCommand(batch, connection)
                {
                    CommandTimeout = 120
                };
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            logger.LogInformation("已套用 {File}。", fileName);
        }
    }

    private static void ValidateDatabaseName(string databaseName)
    {
        // 避免將任意字串拼進動態 SQL；資料庫名稱僅允許常見安全字元。
        if (!Regex.IsMatch(databaseName, @"^[a-zA-Z0-9_]+$"))
            throw new InvalidOperationException($"資料庫名稱格式不正確：{databaseName}");
    }

    private static async Task EnsureDatabaseExistsAsync(SqlConnection connection, string databaseName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            """
            IF DB_ID(@name) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@name);
                EXEC sp_executesql @sql;
            END
            """,
            connection);
        cmd.Parameters.AddWithValue("@name", databaseName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> SchemaIsCurrentAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            """
            SELECT CASE
                WHEN OBJECT_ID(N'dbo.Users', N'U') IS NULL THEN 0
                WHEN COL_LENGTH('dbo.Users', 'Account') IS NULL THEN 0
                WHEN OBJECT_ID(N'dbo.UserDebitAccount', N'U') IS NOT NULL THEN 0
                ELSE 1
            END;
            """,
            connection);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is int i && i == 1;
    }

    private static IEnumerable<string> SplitSqlBatches(string script)
    {
        return GoBatchRegex()
            .Split(script)
            .Select(static b => b.Trim())
            .Where(static b => b.Length > 0);
    }
}
