using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;

namespace Pickbyopen.Database
{
    public class LogRepository(IDbConnectionFactory connectionFactory) : ILogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // <summary>
        // Save a log into the database
        // </summary>
        public async Task SaveLog(Log log)
        {
            using var connection = _connectionFactory.GetConnection();
            await connection.OpenAsync();

            string query = log switch
            {
                SysLog sysLog =>
                    "INSERT INTO SysLogs (CreatedAt, Event, Target, Device) VALUES (@CreatedAt, @Event, @Target, @Device)",
                UserLog userLog =>
                    "INSERT INTO UserLogs (CreatedAt, Event, Target, UserId) VALUES (@CreatedAt, @Event, @Target, @UserId)",
                Operation Operation =>
                    "INSERT INTO Operations (CreatedAt, Event, Target, Door, Mode) VALUES (@CreatedAt, @Event, @Target, @Door, @Mode)",
                _ => throw new InvalidOperationException("Tipo de log desconhecido"),
            };

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("CreatedAt", log.CreatedAt);
            command.Parameters.AddWithValue("Event", log.Event);
            command.Parameters.AddWithValue("Target", log.Target);

            switch (log)
            {
                case SysLog sysLog:
                    command.Parameters.AddWithValue("Device", sysLog.Device);
                    break;
                case UserLog userLog:
                    userLog.User ??= new("0", "0", "0", []); // Avoid null reference exception
                    command.Parameters.AddWithValue("UserId", userLog.User.Id);
                    break;
                case Operation Operation:
                    command.Parameters.AddWithValue("Door", Operation.Door);
                    command.Parameters.AddWithValue("Mode", Operation.Mode);
                    break;
            }

            await command.ExecuteNonQueryAsync();
        }

        // <summary>
        // Load logs from the database
        // </summary>
        public async Task<List<Log>> LoadLogs()
        {
            var logs = new List<Log>();
            using var connection = _connectionFactory.GetConnection();
            await connection.OpenAsync();

            string[] queries =
            {
                "SELECT CreatedAt, Event, Target, Device FROM SysLogs ORDER BY CreatedAt DESC LIMIT 200",
                "SELECT CreatedAt, Event, Target, UserId FROM UserLogs ORDER BY CreatedAt DESC LIMIT 200",
                "SELECT CreatedAt, Event, Target, Door, Mode FROM Operations ORDER BY CreatedAt DESC LIMIT 200",
            };

            foreach (var query in queries)
            {
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                UserRepository userRepository = new(_connectionFactory);

                while (await reader.ReadAsync())
                {
                    Log log = query switch
                    {
                        "SELECT CreatedAt, Event, Target, Device FROM SysLogs ORDER BY CreatedAt DESC LIMIT 200" =>
                            new SysLog(
                                reader.GetDateTime(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3)
                            ),
                        "SELECT CreatedAt, Event, Target, UserId FROM UserLogs ORDER BY CreatedAt DESC LIMIT 200" =>
                            new UserLog(
                                reader.GetDateTime(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                await userRepository.GetUserById(reader.GetString(3))
                                    ?? new User("0", "0", "0", [])
                            ),
                        "SELECT CreatedAt, Event, Target, Door, Mode FROM Operations ORDER BY CreatedAt DESC LIMIT 200" =>
                            new Operation(
                                reader.GetDateTime(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetString(4)
                            ),
                        _ => throw new InvalidOperationException("Tipo de log desconhecido"),
                    };
                    logs.Add(log);
                }
            }

            return logs;
        }

        // <summary>
        // Log a user login
        // </summary>
        public async Task LogUserLogin(User user)
        {
            UserLog userLog = new(DateTime.Now, "Usuário logado", user.Username, user);
            await SaveLog(userLog);
        }

        // <summary>
        // Log a user logout
        // </summary>
        public async Task LogUserLogout(User user)
        {
            UserLog userLog = new(DateTime.Now, "Usuário deslogado", user.Username, user);
            await SaveLog(userLog);
        }

        // <summary>
        // Log a user operation
        // </summary>
        public async Task LogUserOperate(string context, string target, string door, string mode)
        {
            Operation Operation = new(DateTime.Now, context, target, door, mode);
            await SaveLog(Operation);
        }

        // <summary>
        // Log a user create or update a partnumber
        // </summary>
        public async Task LogUserEditPartnumber(User user, string partnumber, string context)
        {
            string msg = context switch
            {
                "create" => "Partnumber cadastrado",
                "update" => "Partnumber alterado",
                _ => throw new InvalidOperationException("Contexto desconhecido"),
            };
            UserLog userLog = new(DateTime.Now, msg, partnumber, user);
            await SaveLog(userLog);
        }

        // <summary>
        // Log a user delete a partnumber
        // </summary>
        public async Task LogUserDeletePartnumber(User user, string partnumber)
        {
            UserLog userLog = new(DateTime.Now, "Partnumber deletado", partnumber, user);
            await SaveLog(userLog);
        }

        // <sumary>
        // Log a user create or update a user
        // </sumary>
        public async Task LogUserEditUser(User user, string target, string context)
        {
            string msg = context switch
            {
                "create" => "Usuário cadastrado",
                "update" => "Usuário alterado",
                _ => throw new InvalidOperationException("Contexto desconhecido"),
            };
            UserLog userLog = new(DateTime.Now, msg, target, user);
            await SaveLog(userLog);
        }

        // <summary>
        // Log a user delete a user
        // </summary>
        public async Task LogUserDeleteUser(User user, string target)
        {
            UserLog userLog = new(DateTime.Now, "Usuário deletado", target, user);
            await SaveLog(userLog);
        }

        // <summary>
        // Log a system operation mode change
        // </summary>
        public async Task LogSysSwitchedMode(string mode)
        {
            SysLog sysLog = new(DateTime.Now, "Modo alterado", mode, "");
            await SaveLog(sysLog);
        }

        // <summary>
        // Log a system PLC status change
        // </summary>
        public async Task LogSysPlcStatusChanged(string status)
        {
            SysLog sysLog = new(DateTime.Now, "Status do PLC alterado", status, "PLC");
            await SaveLog(sysLog);
        }
    }
}
