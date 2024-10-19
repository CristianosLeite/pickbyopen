using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;
using Pickbyopen.Types;

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
                    "INSERT INTO Operations (CreatedAt, Event, Target, Door, Mode, UserId) VALUES (@CreatedAt, @Event, @Target, @Door, @Mode, @UserId)",
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
                    command.Parameters.AddWithValue("UserId", Operation.UserId);
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
                "SELECT CreatedAt, Event, Target, Door, Mode, UserId FROM Operations ORDER BY CreatedAt DESC LIMIT 200",
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
                        "SELECT CreatedAt, Event, Target, Door, Mode, UserId FROM Operations ORDER BY CreatedAt DESC LIMIT 200" =>
                            new Operation(
                                reader.GetDateTime(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetString(4),
                                reader.GetString(5)
                            ),
                        _ => throw new InvalidOperationException("Tipo de log desconhecido"),
                    };
                    logs.Add(log);
                }
            }

            return logs;
        }

        // <summary>
        // Get user logs by date
        // </summary>
        public async Task<List<UserLog>> GetUserLogsByDate(string initialDate, string finalDate)
        {
            var userLogs = new List<UserLog>();

            using var connection = _connectionFactory.GetConnection();
            connection.Open();

            var query =
                @"SELECT userlogs.*, users.username
                    FROM UserLogs
                    JOIN Users ON userlogs.userid = users.id
                    WHERE CreatedAt BETWEEN @initialDate::timestamp AND @finalDate::timestamp
                    ORDER BY CreatedAt DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@initialDate", DateTime.Parse(initialDate));
            command.Parameters.AddWithValue("@finalDate", DateTime.Parse(finalDate).AddDays(1.0));
            command.ExecuteNonQuery();

            using var reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                var createdAt = reader.GetDateTime(1);
                var @event = reader.GetString(2);
                var target = reader.GetString(3);
                var userId = reader.GetString(4);
                var username = reader.GetString(5);

                userLogs.Add(
                    new UserLog(createdAt, @event, target, new User(userId, "0", username, []))
                );
            }

            return userLogs;
        }

        // <summary>
        // Get system logs by date
        // </summary>
        public async Task<List<SysLog>> GetSysLogsByDate(string initialDate, string finalDate)
        {
            var sysLogs = new List<SysLog>();

            using var connection = _connectionFactory.GetConnection();
            connection.Open();

            var query =
                @"SELECT *
                    FROM SysLogs
                    WHERE CreatedAt BETWEEN @initialDate::timestamp AND @finalDate::timestamp
                    ORDER BY CreatedAt DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@initialDate", DateTime.Parse(initialDate));
            command.Parameters.AddWithValue("@finalDate", DateTime.Parse(finalDate).AddDays(1.0));
            command.ExecuteNonQuery();

            using var reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                var createdAt = reader.GetDateTime(1);
                var @event = reader.GetString(2);
                var target = reader.GetString(3);
                var device = reader.GetString(4);

                sysLogs.Add(new SysLog(createdAt, @event, target, device));
            }

            return sysLogs;
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
        public async Task LogUserOperate(
            string @event,
            string target,
            string door,
            string mode,
            string userId
        )
        {
            Operation Operation = new(DateTime.Now, @event, target, door, mode, userId);
            await SaveLog(Operation);
        }

        // <summary>
        // Log a user create or update a partnumber
        // </summary>
        public async Task LogUserEditPartnumber(User user, string partnumber, Context context)
        {
            string msg = context switch
            {
                Context.Create => "Partnumber cadastrado",
                Context.Update => "Partnumber alterado",
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
        public async Task LogUserEditUser(User user, string target, Context context)
        {
            string msg = context switch
            {
                Context.Create => "Usuário cadastrado",
                Context.Update => "Usuário alterado",
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
