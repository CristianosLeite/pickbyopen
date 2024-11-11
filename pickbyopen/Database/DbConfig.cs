using Pickbyopen.Interfaces;

namespace Pickbyopen.Database
{
    public class DbConfig : IDbConfig
    {
        public static string? ConnectionString { get; set; } =
            Environment.GetEnvironmentVariable("PICKBYOPEN_DB_CONNECTION");
    }
}
