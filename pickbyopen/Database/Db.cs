using Npgsql;
using Pickbyopen.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace Pickbyopen.Database
{
    public class Db(string connectionString)
    {
        private readonly string _connectionString = connectionString;

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
                CreatePartnumberTable();

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
        // Insert a partnumber into the partnumbers table or update it if it already exists
        // </summary>
        private static void InsertOrUpdatePartnumber(
            Partnumber partnumber,
            NpgsqlConnection connection
        )
        {
            try
            {
                var insertOrUpdateCommand = new NpgsqlCommand(
                    "INSERT INTO public.partnumbers (partnumber, description) "
                        + "VALUES (@partnumber, @description) "
                        + "ON CONFLICT (partnumber) DO UPDATE "
                        + "SET description = @description;",
                    connection
                );
                insertOrUpdateCommand.Parameters.AddWithValue("@partnumber", partnumber.Code);
                insertOrUpdateCommand.Parameters.AddWithValue(
                    "@desciption",
                    partnumber.Description
                );
                insertOrUpdateCommand.ExecuteNonQuery();
            }
            catch (PostgresException)
            {
                ShowErrorMessage("Não foi possível cadastrar o partnumber.");
            }
        }

        // <summary>
        // Check if a partnumber already exists in the database
        // </summary>
        private static async Task<bool> PartnumberExists(
            NpgsqlConnection connection,
            string partnumber
        )
        {
            try
            {
                var selectRecipe = new NpgsqlCommand(
                    "SELECT 1 FROM public.partnumbers_index WHERE partnumber = @partnumber;",
                    connection
                );
                selectRecipe.Parameters.AddWithValue("@partnumber", partnumber);

                using var reader = await selectRecipe.ExecuteReaderAsync();
                return reader.HasRows;
            }
            catch (PostgresException e)
            {
                throw new Exception("Erro ao verificar se o partnumber já existe." + e);
            }
        }

        // <summary>
        // Insert or update an association between a partnumber and a door
        // </summary>
        private static async Task InsertOrUpdateAssociation(
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
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar ou atualizar a associação." + e);
            }
        }

        // <summary>
        // Insert a new partnumber into the partnumbers table
        // </summary>
        private static async Task InsertNewPartnumber(
            NpgsqlConnection connection,
            string partnumber,
            string description
        )
        {
            try
            {
                var insertPartnumber = new NpgsqlCommand(
                    "INSERT INTO public.partnumbers (partnumber, description) VALUES (@partnumber, @description);",
                    connection
                );
                insertPartnumber.Parameters.AddWithValue("@partnumber", partnumber);
                insertPartnumber.Parameters.AddWithValue("@description", description);

                await insertPartnumber.ExecuteNonQueryAsync();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível cadastrar o partnumber." + e);
            }
        }

        // <summary>
        // Save a list of partnumbers into the database
        // </summary>
        public bool SavePartnumber(List<Partnumber> partnumberList)
        {
            if (partnumberList == null || partnumberList.Count == 0)
            {
                ShowErrorMessage("Nenhum partnumber foi informado.");
                return false;
            }

            try
            {
                using var connection = GetConnection();
                connection.Open();

                foreach (var partnumber in partnumberList)
                    InsertOrUpdatePartnumber(partnumber, connection);
            }
            catch (PostgresException)
            {
                ShowErrorMessage("Não foi possível cadastrar o partnumber.");
                return false;
            }

            return true;
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
        public async Task<bool> InsertPartNumber(string partnumber, string description, string door)
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

            CreatePartnumberIndex();

            using var connection = GetConnection();
            await connection.OpenAsync();

            if (await PartnumberExists(connection, partnumber))
            {
                throw new Exception(
                    "TbPartnumber já cadastrado, se quiser alterá-lo clique em editar"
                );
            }

            await InsertNewPartnumber(connection, partnumber, description);

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
                CreatePartnumberTable();

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
        public bool DeletePartnumber(string partnumber)
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
        public void DeletePartnumberIndex(string partnumber)
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
                CreatePartnumberIndex();

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

        public ObservableCollection<User> LoadUsersList()
        {
            try
            {
                CreateUsersTable();

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

        public async Task<User?> GetUser(string id)
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

        public async Task<bool> SaveUser(User user)
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

                return true;
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao cadastrar o usuário." + e);
                return false;
            }
        }

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

        public bool DeleteUser(string id)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var deleteUser = new NpgsqlCommand("DELETE FROM users WHERE id = @id;", connection);
                deleteUser.Parameters.AddWithValue("@id", id);

                deleteUser.ExecuteNonQuery();
                return true;
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Erro ao deletar o usuário." + e);
                return false;
            }
        }
    }
}
