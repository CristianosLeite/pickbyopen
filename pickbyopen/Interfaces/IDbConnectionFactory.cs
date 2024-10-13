using Npgsql;

namespace Pickbyopen.Interfaces
{
    public interface IDbConnectionFactory
    {
        NpgsqlConnection GetConnection();
    }
}
