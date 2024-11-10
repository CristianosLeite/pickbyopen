using System.Collections.ObjectModel;
using System.Diagnostics;
using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;

namespace Pickbyopen.Database
{
    internal class OperationRepository(IDbConnectionFactory connectionFactory)
        : IOperationRepository
    {
        public async Task<ObservableCollection<Operation>> LoadOperations()
        {
            try
            {
                var operations = new ObservableCollection<Operation>();

                using var connection = connectionFactory.GetConnection();
                {
                    connection.Open();
                    using var command = new NpgsqlCommand(
                        "SELECT createdat, event, target, chassi, door, mode, userid FROM operations ORDER BY id DESC",
                        connection
                    );
                    using var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var createdAt = reader.GetDateTime(0);
                        var @event = reader.GetString(1);
                        var target = reader.GetString(2);
                        var chassi = reader.GetString(3);
                        var door = reader.GetString(4);
                        var mode = reader.GetString(5);
                        var userId = reader.GetString(6);

                        operations.Add(
                            new Operation(createdAt, @event, target, chassi, door, mode, userId)
                        );
                    }
                }

                return operations;
            }
            catch (PostgresException e)
            {
                throw new Exception("Erro ao carregar a lista de operações." + e);
            }
        }

        public async Task<List<Operation>> GetOperationsByDate(
            string target,
            string chassi,
            string door,
            string initialDate,
            string finalDate
        )
        {
            var operations = new List<Operation>();

            using var connection = connectionFactory.GetConnection();
            connection.Open();

            var query = BuildQuery(target, door);
            using var command = new NpgsqlCommand(query, connection);
            AddParameters(command, target, chassi, door, initialDate, finalDate);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var createdAt = reader.GetDateTime(1);
                var @event = reader.GetString(2);
                var _target = reader.GetString(3);
                var _chassi = reader.GetString(4);
                var _door = reader.GetString(5);
                var mode = reader.GetString(6);
                var userId = reader.GetString(7);
                var username = reader.GetString(8);

                operations.Add(
                    new Operation(createdAt, @event, _target, _chassi, _door, mode, userId, username)
                );
            }

            return operations;
        }

        private static string BuildQuery(string target, string door)
        {
            var baseQuery =
                @"SELECT operations.*, users.username FROM operations
                      JOIN users ON operations.userid = users.id
                      WHERE createdat BETWEEN @initialDate AND @finalDate";

            if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(door))
            {
                return baseQuery + " AND target = @target AND door = @door ORDER BY id DESC";
            }
            else if (!string.IsNullOrEmpty(target))
            {
                return baseQuery + " AND target = @target ORDER BY id DESC";
            }
            else if (!string.IsNullOrEmpty(door))
            {
                return baseQuery + " AND door = @door ORDER BY id DESC";
            }
            else
            {
                return baseQuery + " ORDER BY id DESC";
            }
        }

        private static void AddParameters(
            NpgsqlCommand command,
            string target,
            string chassi,
            string door,
            string initialDate,
            string finalDate
        )
        {
            if (!string.IsNullOrEmpty(target))
            {
                command.Parameters.AddWithValue("@target", target);
            }

            if (chassi != null) { 
                command.Parameters.AddWithValue("@chassi", chassi);
            }

            if (!string.IsNullOrEmpty(door))
            {
                command.Parameters.AddWithValue("@door", door);
            }

            command.Parameters.AddWithValue("@initialDate", DateTime.Parse(initialDate));
            command.Parameters.AddWithValue("@finalDate", DateTime.Parse(finalDate).AddDays(1.0));
        }
    }
}
