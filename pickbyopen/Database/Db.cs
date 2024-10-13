﻿using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;
using Pickbyopen.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Pickbyopen.Types;

namespace Pickbyopen.Database
{
    public class Db : DatabaseConfig
    {
        protected readonly IDbConnectionFactory _connectionFactory;
        protected readonly IPartnumberRepository _partnumberRepository;
        protected readonly IUserRepository _userRepository;
        protected readonly ILogRepository _logRepository;

        public Db(
            IDbConnectionFactory connectionFactory,
            IPartnumberRepository partnumberRepository,
            IUserRepository userRepository,
            ILogRepository logRepository
        )
        {
            _connectionFactory = connectionFactory;
            _partnumberRepository = partnumberRepository;
            _userRepository = userRepository;
            _logRepository = logRepository;

            CreateUsersTable();
            CreatePartnumberTable();
            CreatePartnumberIndex();
            CreateSysLogTable();
            CreateUserLogTable();
            CreateOperationTable();
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
        // Create UserLogs table
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

        public async Task<bool> SavePartnumber(string partnumber, string description, string door)
        {
            await _logRepository.LogUserEditPartnumber(Auth.LoggedInUser!, partnumber, Context.Create);
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

        public ObservableCollection<string> LoadAvailablePartnumbers()
        {
            return _partnumberRepository.LoadAvailablePartnumbers();
        }

        public ObservableCollection<string> LoadAssociatedPartnumbers(string door)
        {
            return _partnumberRepository.LoadAssociatedPartnumbers(door);
        }

        public async Task<int> GetAssociatedDoor(string partnumber)
        {
            return await _partnumberRepository.GetAssociatedDoor(partnumber);
        }

        public async Task<bool> CreateAssociation(string partnumber, string door)
        {
            await _logRepository.LogUserEditPartnumber(Auth.LoggedInUser!, partnumber, Context.Update);
            return await _partnumberRepository.CreateAssociation(partnumber, door);
        }

        public async Task<bool> DeletePartnumberIndex(string partnumber)
        {
            await _logRepository.LogUserEditPartnumber(Auth.LoggedInUser!, partnumber, Context.Update);
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

        public async Task LogUserOperate(string context, string target, string door, string mode)
        {
            await _logRepository.LogUserOperate(context, target, door, mode);
        }

        public async Task LogSysPlcStatusChanged(string status)
        {
            await _logRepository.LogSysPlcStatusChanged(status);
        }

        public async Task LogSysSwitchedMode(string mode)
        {
            await _logRepository.LogSysSwitchedMode(mode);
        }
    }
}
