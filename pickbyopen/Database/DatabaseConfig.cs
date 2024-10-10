namespace Pickbyopen.Database
{
    public class DatabaseConfig
    {
        public static string? ConnectionString { get; } = Environment.GetEnvironmentVariable("PICKBYOPEN_DB_CONNECTION");
    }
}
