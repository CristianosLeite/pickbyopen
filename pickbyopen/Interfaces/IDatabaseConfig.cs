namespace Pickbyopen.Interfaces
{
    internal interface IDatabaseConfig
    {
        public static string? ConnectionString { get; } =
            Environment.GetEnvironmentVariable("PICKBYOPEN_DB_CONNECTION");
    }
}
