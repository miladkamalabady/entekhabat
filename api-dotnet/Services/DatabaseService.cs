using Microsoft.Data.SqlClient;

namespace EntekhabatApi.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    // یک connection جدید بدون باز کردن
    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    // یک connection که از قبل باز است - برای transaction ها
    public async Task<SqlConnection> OpenConnectionAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
