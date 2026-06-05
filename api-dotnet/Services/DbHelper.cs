using System.Data;
using Dapper;
using MySqlConnector;

namespace EntekhabatApi.Services;

/// <summary>
/// اجرای امن transaction - از تداخل await using + try-catch جلوگیری می‌کند.
/// اگر callback موفق باشد commit می‌کند، در غیر این صورت rollback.
/// </summary>
public static class DbHelper
{
    public static async Task<T> WithTransaction<T>(
        MySqlConnection conn,
        Func<MySqlTransaction, Task<T>> callback)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        var tx = await conn.BeginTransactionAsync();
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
        finally
        {
            await tx.DisposeAsync();
        }
    }

    public static async Task WithTransaction(
        MySqlConnection conn,
        Func<MySqlTransaction, Task> callback)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        var tx = await conn.BeginTransactionAsync();
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
        finally
        {
            await tx.DisposeAsync();
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
