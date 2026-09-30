using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace EntekhabatApi.Services;

/// <summary>
/// اجرای امن transaction - از تداخل await using + try-catch جلوگیری می‌کند.
/// اگر callback موفق باشد commit می‌کند، در غیر این صورت rollback.
/// </summary>
public static class DbHelper
{
    public static async Task<T> WithTransaction<T>(
        SqlConnection conn,
        Func<SqlTransaction, Task<T>> callback)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();
        try
        {
            var result = await callback(tx);
            await tx.CommitAsync();
            return result;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public static async Task WithTransaction(
        SqlConnection conn,
        Func<SqlTransaction, Task> callback)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();
        try
        {
            await callback(tx);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // Dapper نمی‌تواند IDictionary را مستقیم بسازد؛ باید dynamic بگیریم و cast کنیم
    public static async Task<IDictionary<string, object>?> QueryRowDict(
        this IDbConnection conn, string sql, object? param = null, IDbTransaction? tx = null)
    {
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, param, tx);
        if (row == null) return null;
        return (IDictionary<string, object>)row;
    }

    public static async Task<IList<IDictionary<string, object>>> QueryListDict(
        this IDbConnection conn, string sql, object? param = null, IDbTransaction? tx = null)
    {
        var rows = await conn.QueryAsync<dynamic>(sql, param, tx);
        return rows.Select(r => (IDictionary<string, object>)r).ToList();
    }

    // دسترسی امن به IDictionary که Dapper برمی‌گرداند
    public static object? GetValueOrDefault(this IDictionary<string, object> d, string key)
    {
        d.TryGetValue(key, out var val);
        return val is DBNull ? null : val;
    }

    public static T? Get<T>(this IDictionary<string, object> d, string key)
    {
        var val = d.GetValueOrDefault(key);
        if (val == null) return default;
        try { return (T)Convert.ChangeType(val, typeof(T)); }
        catch { return default; }
    }

    public static string Str(this IDictionary<string, object> d, string key)
    {
        var val = d.GetValueOrDefault(key);
        if (val == null) return "";
        if (val is string s) return s;
        if (val is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        return val.ToString() ?? "";
    }

    public static int Int(this IDictionary<string, object> d, string key)
        => Convert.ToInt32(d.GetValueOrDefault(key) ?? 0);
}
