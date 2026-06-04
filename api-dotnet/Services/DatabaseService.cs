using MySqlConnector;

namespace EntekhabatApi.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);
}
