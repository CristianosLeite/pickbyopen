using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;
using Pickbyopen.Services;
using Pickbyopen.Types;
using System.Collections.ObjectModel;
using System.Windows;

namespace Pickbyopen.Database
{
    public class Db : DatabaseConfig
    {
        protected readonly IDbConnectionFactory _connectionFactory;
        protected readonly IPartnumberRepository _partnumberRepository;
        protected readonly IUserRepository _userRepository;
        protected readonly ILogRepository _logRepository;
        protected readonly IOperationRepository _operationRepository;
        protected readonly IRecipeRepository _recipeRepository;

        public Db(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

            _partnumberRepository = new PartnumberRepository(connectionFactory);
            _userRepository = new UserRepository(connectionFactory);
            _logRepository = new LogRepository(connectionFactory);
            _operationRepository = new OperationRepository(connectionFactory);
            _recipeRepository = new RecipeRepository(connectionFactory);

            CreateUsersTable();
            CreatePartnumberTable();
            CreatePartnumberIndex();
            CreateSysLogTable();
            CreateUserLogTable();
            CreateOperationTable();
            CreateRecipeTable();
            CreateRecipePartnumberTable();
        }

        public static NpgsqlConnection GetConnection()
        {
            try
            {
                var connectionString = ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Database connection string is not configured."
                    );
                }

                return new NpgsqlConnection(connectionString);
            }
            catch (ArgumentNullException ex)
            {
                ShowErrorMessage("Erro ao obter a string de conexão: " + ex.Message);
                App.Current.Shutdown();
                throw;
            }
            catch (InvalidOperationException ex)
            {
                ShowErrorMessage("Erro de configuração: " + ex.Message);
                App.Current.Shutdown();
                throw;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erro ao criar a conexão com o banco de dados: " + ex.Message);
                App.Current.Shutdown();
                throw;
            }
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
        private static void CreatePartnumberIndex()
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
        private static void CreatePartnumberTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.partnumbers ("
                        + "partnumber_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "partnumber character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "description character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "CONSTRAINT partnumber_pkey PRIMARY KEY (partnumber_id), "
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
        // Create a table for users
        // </summary>
        private static void CreateUsersTable()
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
        private static void CreateSysLogTable()
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
        // Create SysLogs table
        // </summary>
        private static void CreateUserLogTable()
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
        private static void CreateOperationTable()
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
                        + "UserId character varying COLLATE pg_catalog.\"default\" NOT NULL, "
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
        // Create recipe table
        // </summary>
        private static void CreateRecipeTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.Recipes ("
                        + "recipe_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "vp character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "description character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "CONSTRAINT Recipes_pkey PRIMARY KEY (recipe_id)) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.Recipes OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de receitas." + e);
            }
        }

        // <summary>
        // Create recipe partnumber table
        // </summary>
        private static void CreateRecipePartnumberTable()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS public.RecipePartnumber ("
                        + "id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ("
                        + "INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1), "
                        + "vp character varying COLLATE pg_catalog.\"default\" NOT NULL, "
                        + "partnumber_id bigint NOT NULL, "
                        + "recipe_id bigint NOT NULL, "
                        + "CONSTRAINT RecipePartnumber_pkey PRIMARY KEY (id), "
                        + "CONSTRAINT RecipePartnumber_fk FOREIGN KEY (partnumber_id) "
                        + "REFERENCES public.partnumbers (partnumber_id) ON DELETE CASCADE, "
                        + "CONSTRAINT RecipePartnumber_fk_1 FOREIGN KEY (recipe_id) "
                        + "REFERENCES public.recipes (recipe_id) ON DELETE CASCADE) "
                        + "TABLESPACE pg_default; "
                        + "ALTER TABLE IF EXISTS public.RecipePartnumber OWNER to postgres;",
                    connection
                );

                createTableCommand.ExecuteNonQuery();
            }
            catch (PostgresException e)
            {
                ShowErrorMessage("Não foi possível criar a tabela de receitas de partnumber." + e);
            }
        }

        public async Task<bool> SavePartnumber(string partnumber, string description, string door)
        {
            await _logRepository.LogUserEditPartnumber(
                Auth.LoggedInUser!,
                partnumber,
                Context.Create
            );
            return await _partnumberRepository.SavePartnumber(partnumber, description, door);
        }

        public async Task<bool> DeletePartnumber(string partnumber)
        {
            await _logRepository.LogUserDeletePartnumber(Auth.LoggedInUser!, partnumber);
            return await _partnumberRepository.DeletePartnumber(partnumber);
        }

        public ObservableCollection<Partnumber> LoadPartnumberList()
        {
            return _partnumberRepository.LoadPartnumberList();
        }

        public async Task<ObservableCollection<string>> LoadAvailablePartnumbers()
        {
            return await _partnumberRepository.LoadAvailablePartnumbers();
        }

        public async Task<ObservableCollection<string>> LoadAssociatedPartnumbers(string door)
        {
            return await _partnumberRepository.LoadAssociatedPartnumbers(door);
        }

        public async Task<int> GetAssociatedDoor(string partnumber)
        {
            return await _partnumberRepository.GetAssociatedDoor(partnumber);
        }

        public async Task<bool> CreateAssociation(string partnumber, string door)
        {
            await _logRepository.LogUserEditPartnumber(
                Auth.LoggedInUser!,
                partnumber,
                Context.Update
            );
            return await _partnumberRepository.CreateAssociation(partnumber, door);
        }

        public async Task<bool> DeletePartnumberIndex(string partnumber)
        {
            await _logRepository.LogUserEditPartnumber(
                Auth.LoggedInUser!,
                partnumber,
                Context.Update
            );
            return await _partnumberRepository.DeletePartnumberIndex(partnumber);
        }

        public ObservableCollection<User> LoadUsersList()
        {
            return _userRepository.LoadUsersList();
        }

        public Task<User?> FindUserByBadgeNumber(string badgeNumber)
        {
            return _userRepository.FindUserByBadgeNumber(badgeNumber);
        }

        public async Task<User?> GetUserById(string id)
        {
            return await _userRepository.GetUserById(id);
        }

        public async Task<bool> SaveUser(User user, Context context)
        {
            await _logRepository.LogUserEditUser(Auth.LoggedInUser!, user.Username, context);
            return await _userRepository.SaveUser(user, context);
        }

        public async Task<bool> DeleteUser(User user)
        {
            await _logRepository.LogUserDeleteUser(Auth.LoggedInUser!, user.Username);
            return await _userRepository.DeleteUser(user);
        }

        public async Task<List<Log>> LoadLogs()
        {
            return await _logRepository.LoadLogs();
        }

        public async Task LogUserLogin(User user)
        {
            await _logRepository.LogUserLogin(user);
        }

        public async Task LogUserLogout(User user)
        {
            await _logRepository.LogUserLogout(user);
        }

        public async Task LogUserOperate(
            string context,
            string target,
            string door,
            string mode,
            string userId
        )
        {
            await _logRepository.LogUserOperate(context, target, door, mode, userId);
        }

        public async Task<List<UserLog>> GetUserLogsByDate(string initialDate, string finalDate)
        {
            return await _logRepository.GetUserLogsByDate(initialDate, finalDate);
        }

        public async Task LogSysPlcStatusChanged(string status)
        {
            await _logRepository.LogSysPlcStatusChanged(status);
        }

        public async Task LogSysSwitchedMode(string mode)
        {
            await _logRepository.LogSysSwitchedMode(mode);
        }

        public async Task<List<SysLog>> GetSysLogsByDate(string initialDate, string finalDate)
        {
            return await _logRepository.GetSysLogsByDate(initialDate, finalDate);
        }

        public async Task<ObservableCollection<Operation>> LoadOperations()
        {
            return await _operationRepository.LoadOperations();
        }

        public async Task<List<Operation>> GetOperationsByDate(
            string partnumber,
            string door,
            string initialDate,
            string finalDate
        )
        {
            return await _operationRepository.GetOperationsByDate(
                partnumber,
                door,
                initialDate,
                finalDate
            );
        }

        public async Task<bool> SaveRecipe(
            Recipe recipe,
            List<Partnumber> partnumbers,
            Context context
        )
        {
            return await _recipeRepository.SaveRecipe(recipe, partnumbers, context);
        }

        public async Task<bool> CreateRecipePartnumberAssociation(
            string vp,
            int partnumberId,
            int recipeId
        )
        {
            return await _recipeRepository.CreateRecipePartnumberAssociation(
                vp,
                partnumberId,
                recipeId
            );
        }

        public ObservableCollection<Recipe> LoadRecipeList()
        {
            return _recipeRepository.LoadRecipeList();
        }

        public async Task<Recipe?> GetRecipeByVp(string vp)
        {
            return await _recipeRepository.GetRecipeByVp(vp);
        }

        public async Task<List<int>> GetRecipeAssociatedDoors(string vp)
        {
            return await _recipeRepository.GetRecipeAssociatedDoors(vp);
        }

        public async Task<bool> DeleteRecipePartnumberAssociation(string vp)
        {
            return await _recipeRepository.DeleteRecipePartnumberAssociation(vp);
        }

        public async Task<bool> DeleteRecipe(string vp)
        {
            return await _recipeRepository.DeleteRecipe(vp);
        }
    }
}
