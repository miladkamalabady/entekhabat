using MySqlConnector;

namespace EntekhabatApi.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    // یک connection جدید بدون باز کردن
    public MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);

    // یک connection که از قبل باز است - برای transaction ها
    public async Task<MySqlConnection> OpenConnectionAsync()
    {
        var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
