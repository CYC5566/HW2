using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace USUN2.Data.Infrastructure;

public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}
