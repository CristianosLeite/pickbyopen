using Npgsql;
using Pickbyopen.Interfaces;

namespace Pickbyopen.Database
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        public NpgsqlConnection GetConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("PICKBYOPEN_DB_CONNECTION");
            return new NpgsqlConnection(connectionString);
        }
    }
}
