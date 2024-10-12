using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using Npgsql;
using Pickbyopen.Models;
using Pickbyopen.Services;

namespace Pickbyopen.Database
{
    public partial class Db
    {
        private readonly string _connectionString;

        public Db(string connectionString)
        {
            _connectionString = connectionString;

            CreateUsersTable();
            CreatePartnumberTable();
            CreatePartnumberIndex();
            CreateSysLogTable();
            CreateUserLogTable();
            CreateOperationTable();
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // <summary>
        // Show an error message
        // </summary>
        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // <summary>
        // Create an index table for partnumber and doors association
        // </summary>
        private void CreatePartnumberIndex()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createIndexCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.partnumbers_index ("
                        + "id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "partnumber character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "door character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "CONSTRAINT partnumber_index_pkey PRIMARY KEY (id), "
                        + "CONSTRAINT \"UQ_associateted\" UNIQUE (partnumber), "
                        + "CONSTRAINT partnumber_fk FOREIGN KEY (partnumber) "
                        + "REFERENCES public.partnumbers (partnumber)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.partnumbers_index OWNER to postgres;",
                    connection
                );
                createIndexCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de índice de partnumber." + e);
            }
        }

        // <summary>
        // Create a table for partnumber
        // </summary>
        private void CreatePartnumberTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.partnumbers ("
                        + "id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "partnumber character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "description character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "CONSTRAINT partnumber_pkey PRIMARY KEY (id), "
                        + "CONSTRAINT \"UQ_partnumber\" UNIQUE (partnumber)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.partnumbers OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de partnumber." + e);
            }
        }

        // <summary>
        //Create a table for users
        // </summary>
        private void CreateUsersTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.users ("
                        + "id character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "badge_number character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "username character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "permissions character varying[] COLLATE pg_catalog.\"default\", "
                        + "CONSTRAINT users_pkey PRIMARY KEY (id)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.users OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de usuários." + e);
            }
        }

        // <summary>
        // Create SysLogs table
        // </summary>
        private void CreateSysLogTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.SysLogs ("
                        + "id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "CreatedAt timestamp without time zone NOT NULL, "
                        + "Event character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "Target character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "Device character varying COLLATE pg_catalog.\"default\", "
                        + "CONSTRAINT SysLogs_pkey PRIMARY KEY (id)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.SysLogs OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de logs do sistema." + e);
            }
        }

        // <summary>
        // Create UserLogs table
        // </summary>
        private void CreateUserLogTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.UserLogs ("
                        + "id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "CreatedAt timestamp without time zone NOT NULL, "
                        + "Event character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "Target character varying COLLATE pg_catalog.\"default\", "
                        + "UserId character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "CONSTRAINT UserLogs_pkey PRIMARY KEY (id)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.UserLogs OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de logs de usuário." + e);
            }
        }

        // <summary>
        // Create operations table
        // </summary>
        private void CreateOperationTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.Operations ("
                        + "id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "CreatedAt timestamp without time zone NOT NULL, "
                        + "Event character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "Target character varying COLLATE pg_catalog.\"default\", "
                        + "Door character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "Mode character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "CONSTRAINT Operations_pkey PRIMARY KEY (id)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.Operations OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de relatórios." + e);
            }
        }

        // <summary>
        // Insert or update an association between a partnumber and a door
        // </summary>
        private async Task InsertOrUpdateAssociation(
            NpgsqlConnection connection,
            string partnumber,
            string door
        )
        {
            try
            {
                var insertOrUpdateAssociation = new NpgsqlCommand(
                    "INSERT INTO public.partnumbers_index (partnumber, door) "
                        + "VALUES (@partnumber, @door) "
                        + "ON CONFLICT (partnumber) DO UPDATE "
                        + "SET door = @door;",
                    connection
                );
                insertOrUpdateAssociation.Parameters.AddWithValue("@partnumber", partnumber);
                insertOrUpdateAssociation.Parameters.AddWithValue("@door", door);

                await insertOrUpdateAssociation.ExecuteNonQueryAsync();
                await LogUserEditPartnumber(Auth.LoggedInUser!, partnumber, "update");
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar ou atualizar a associação." + e);
            }
        }

        // <summary>
        // Insert a new partnumber into the partnumbers table
        // </summary>
        private async Task InsertOrUpdatePartnumber(
            NpgsqlConnection connection,
            string partnumber,
            string description
        )
        {
            try
            {
                var insertPartnumber = new NpgsqlCommand(
                    "INSERT INTO public.partnumbers (partnumber, description) VALUES (@partnumber, @description) "
                        + "ON CONFLICT (partnumber) DO UPDATE "
                        + "SET description = @description;",
                    connection
                );
                insertPartnumber.Parameters.AddWithValue("@partnumber", partnumber);
                insertPartnumber.Parameters.AddWithValue("@description", description);

                await insertPartnumber.ExecuteNonQueryAsync();
                await LogUserEditPartnumber(Auth.LoggedInUser!, partnumber, "create");
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível cadastrar o partnumber." + e);
            }
        }

        // <summary>
        // Create an association between a partnumber and a door
        // </summary>
        public async Task CreateAssociation(string partnumber, string door)
        {
            try
            {
                if (string.IsNullOrEmpty(partnumber))
                    throw new ArgumentNullException(nameof(partnumber));

                if (string.IsNullOrEmpty(door))
                    throw new ArgumentNullException(nameof(door));

                using var connection = GetConnection();
                await connection.OpenAsync();

                await InsertOrUpdateAssociation(connection, partnumber, door);
            }
            catch (Exception e)
            {
                ShowErrorMessage("Não foi possível criar a associação." + e);
            }
        }

        // <summary>
        // Update an association between a partnumber and a door
        // </summary>
        public async Task<bool> UpdateAssociation(
            string partnumber,
            string description,
            string door
        )
        {
            try
            {
                if (string.IsNullOrEmpty(partnumber))
                    throw new ArgumentNullException(nameof(partnumber));

                if (partnumber.Length != 15)
                    throw new ArgumentException(
                        "TbPartnumber deve ter 15 caracteres.",
                        nameof(partnumber)
                    );

                if (string.IsNullOrEmpty(description))
                    throw new ArgumentNullException(nameof(description));

                using var connection = GetConnection();
                await connection.OpenAsync();

                if (!string.IsNullOrEmpty(door))
                    await InsertOrUpdateAssociation(connection, partnumber, door);

                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage("Não foi possível atualizar a associação." + e);
                return false;
            }
        }

        // <summary>
        // Insert a new partnumber into the database
        // </summary>
        public async Task<bool> SavePartnumber(string partnumber, string description, string door)
        {
            if (string.IsNullOrEmpty(partnumber))
                throw new ArgumentNullException(nameof(partnumber));

            if (partnumber.Length != 15)
                throw new ArgumentException(
                    "TbPartnumber deve ter 15 caracteres.",
                    nameof(partnumber)
                );

            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));

            using var connection = GetConnection();
            await connection.OpenAsync();

            await InsertOrUpdatePartnumber(connection, partnumber, description);

            if (!string.IsNullOrEmpty(door))
                await InsertOrUpdateAssociation(connection, partnumber, door);

            return true;
        }

        // <summary>
        // Load a list of partnumbers from the database
        // </summary>
        public ObservableCollection<Partnumber> LoadPartnumberList()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var partnumberList = new ObservableCollection<Partnumber>();

                using var command = new NpgsqlCommand(
                    "SELECT partnumber, description FROM public.partnumbers;",
                    connection
                );
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    partnumberList.Add(new Partnumber(reader.GetString(0), reader.GetString(1)));
                }

                return partnumberList;
            }
            catch (PostgresException e)
            {
                throw new Exception("Erro ao carregar a lista de partnumbers." + e);
            }
        }

        // <summary>
        // Delete a partnumber from the database
        // </summary>
        public async Task<bool> DeletePartnumber(string partnumber)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM public.partnumbers WHERE partnumber = @partnumber;",
                    connection
                );
                deleteCommand.Parameters.AddWithValue("@partnumber", partnumber);
                deleteCommand.ExecuteNonQuery();
                await LogUserDeletePartnumber(Auth.LoggedInUser!, partnumber);
                return true;
            }
            catch (PostgresException)
            {
                ShowErrorMessage(
                    "Não foi possível deletar o partnumber. Verifique se o partnumber está associado à alguma porta e desfaça a associação."
                );
                return false;
            }
        }

        // <summary>
        // Get the door associated with a partnumber
        // </summary>
        public async Task<int> GetAssociatedDoor(string partnumber)
        {
            try
            {
                var connection = GetConnection();
                connection.Open();

                var selectRecipeAssociated = new NpgsqlCommand(
                    "SELECT door FROM public.partnumbers_index WHERE partnumber = @partnumber LIMIT 1;",
                    connection
                );
                selectRecipeAssociated.Parameters.AddWithValue("@partnumber", partnumber);

                var associationExists = await selectRecipeAssociated.ExecuteReaderAsync();

                if (associationExists.HasRows)
                {
                    associationExists.Read();

                    var door = associationExists.GetString(0);

                    connection.Close();

                    return Convert.ToInt32(door);
                }
                else
                {
                    return 0;
                }
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao buscar a associação." + e);
                return 0;
            }
        }

        // <summary>
        // Delete an association between a partnumber and a door
        // </summary>
        public async void DeletePartnumberIndex(string partnumber)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM public.partnumbers_index WHERE partnumber = @partnumber;",
                    connection
                );
                deleteCommand.Parameters.AddWithValue("@partnumber", partnumber);
                deleteCommand.ExecuteNonQuery();
                await LogUserEditPartnumber(Auth.LoggedInUser!, partnumber, "update");
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao deletar a associação." + e);
            }
        }

        // <summary>
        // Load a list of partnumbers without associations
        // </summary>
        public ObservableCollection<string> LoadAvailablePartnumbers()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                DataTable dt = new();

                ObservableCollection<string> ptnList = [];
                using var command = new NpgsqlCommand(
                    "SELECT partnumber FROM public.partnumbers WHERE partnumber NOT IN(SELECT partnumber FROM public.partnumbers_index);",
                    connection
                );
                using var reader = command.ExecuteReader();
                dt.Load(reader);

                foreach (DataRow row in dt.Rows)
                {
                    ptnList.Add(item: row[0].ToString()!);
                }

                return ptnList;
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao carregar a lista de partnumbers." + e);
                return [];
            }
        }

        // <summary>
        // Load a list of partnumbers associated with a door
        // </summary>
        public ObservableCollection<string> LoadAssociatedPartnumbers(string door)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                DataTable dt = new();

                ObservableCollection<string> ptnList = [];

                using var command = new NpgsqlCommand(
                    "SELECT partnumber FROM public.partnumbers_index WHERE door = @door;",
                    connection
                );
                command.Parameters.AddWithValue("@door", door);
                using var reader = command.ExecuteReader();

                dt.Load(reader);

                foreach (DataRow row in dt.Rows)
                {
                    ptnList.Add(item: row[0].ToString()!);
                }

                return ptnList;
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao carregar a lista de partnumbers." + e);
                return [];
            }
        }

        //<summary>
        // Load a list of users from the database
        //</summary>
        public ObservableCollection<User> LoadUsersList()
        {
            try
            {
                var usersList = new ObservableCollection<User>();

                using (var connection = GetConnection())
                {
                    connection.Open();
                    using var command = new NpgsqlCommand(
                        "SELECT id, badge_number, username, permissions FROM users",
                        connection
                    );
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var id = reader.GetString(0);
                        var badgeNumber = reader.GetString(1);
                        var username = reader.GetString(2);
                        var permissionsArray = reader.GetFieldValue<string[]>(3);

                        var permissions = permissionsArray.ToList();

                        usersList.Add(new User(id, badgeNumber, username, permissions));
                    }
                }

                return usersList;
            }
            catch (PostgresException e)
            {
                throw new Exception("Erro ao carregar a lista de usuários." + e);
            }
        }

        //<summary>
        // Get a user by id from the database
        //</summary>
        public async Task<User?> GetUserById(string id)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var selectUser = new NpgsqlCommand(
                    "SELECT * FROM users WHERE id = @id;",
                    connection
                );
                selectUser.Parameters.AddWithValue("@id", id);

                using var reader = await selectUser.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    reader.Read();

                    string userId = reader.GetString(0);
                    string badgeNumber = reader.GetString(1);
                    string username = reader.GetString(2);
                    string[] permissionsArray = reader.GetFieldValue<string[]>(3);

                    List<string> permissions = [.. permissionsArray];

                    return new User(id, badgeNumber, username, permissions);
                }
                else
                {
                    return null;
                }
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao buscar o usuário." + e);
                return null;
            }
        }

        //<summary>
        // Save a user into the database
        //</summary>
        public async Task<bool> SaveUser(User user, string context)
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                var insertUser = new NpgsqlCommand(
                    "INSERT INTO users (id, badge_number, username, permissions) "
                        + "VALUES (@id, @badge_number, @username, @permissions) "
                        + "ON CONFLICT (id) DO UPDATE "
                        + "SET badge_number = @badge_number, username = @username, permissions = @permissions;",
                    connection
                );
                insertUser.Parameters.AddWithValue("@id", user.Id);
                insertUser.Parameters.AddWithValue("@badge_number", user.BadgeNumber);
                insertUser.Parameters.AddWithValue("@username", user.Username);
                insertUser.Parameters.AddWithValue("@permissions", user.Permissions);

                await insertUser.ExecuteNonQueryAsync();
                await LogUserEditUser(Auth.LoggedInUser!, user.Username, context);

                return true;
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao cadastrar o usuário." + e);
                return false;
            }
        }

        //<summary>
        // Find a user by badge number
        //</summary>
        public async Task<User?> FindUserByBadgeNumber(string badgeNumber)
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                var selectUser = new NpgsqlCommand(
                    "SELECT * FROM users WHERE badge_number = @badge_number;",
                    connection
                );
                selectUser.Parameters.AddWithValue("@badge_number", badgeNumber);

                using var reader = await selectUser.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    reader.Read();

                    string id = reader.GetString(0);
                    string username = reader.GetString(2);
                    string[] permissionsArray = reader.GetFieldValue<string[]>(3);

                    List<string> permissions = [.. permissionsArray];

                    return new User(id, badgeNumber, username, permissions);
                }
                else
                {
                    return null;
                }
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao buscar o usuário." + e);
                return null;
            }
        }

        //<summary>
        // Delete a user from the database
        //</summary>
        public async Task<bool> DeleteUser(string id)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                User? user =
                    await GetUserById(id)
                    ?? throw new InvalidOperationException("Usuário não encontrado");

                var deleteUser = new NpgsqlCommand("DELETE FROM users WHERE id = @id;", connection);
                deleteUser.Parameters.AddWithValue("@id", id);

                await LogUserDeleteUser(Auth.LoggedInUser!, user.Username);
                return true;
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao deletar o usuário." + e);
                return false;
            }
        }

        // <summary>
        // Save a log into the database
        // </summary>
        public async Task SaveLog(Log log)
        {
            using var connection = GetConnection();
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
                    if (userLog.User == null)
                    {
                        userLog.User = new User("0", "0", "0", []);
                    }
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
            using var connection = GetConnection();
            await connection.OpenAsync();

            string[] queries =
            {
                "SELECT CreatedAt, Event, Target, Device FROM SysLogs ORDER BY CreatedAt DESC",
                "SELECT CreatedAt, Event, Target, UserId FROM UserLogs ORDER BY CreatedAt DESC",
                "SELECT CreatedAt, Event, Target, Door, Mode FROM Operations ORDER BY CreatedAt DESC",
            };

            foreach (var query in queries)
            {
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Log log = query switch
                    {
                        "SELECT CreatedAt, Event, Target, Device FROM SysLogs ORDER BY CreatedAt DESC" =>
                            new SysLog(
                                reader.GetDateTime(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3)
                            ),
                        "SELECT CreatedAt, Event, Target, UserId FROM UserLogs ORDER BY CreatedAt DESC" =>
                            new UserLog(
                                reader.GetDateTime(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                await GetUserById(reader.GetString(3))
                                    ?? new User("0", "0", "0", [])
                            ),
                        "SELECT CreatedAt, Event, Target, Door, Mode FROM Operations ORDER BY CreatedAt DESC" =>
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
