using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;
using Pickbyopen.Utils;
using System.Collections.ObjectModel;
using System.Data;

namespace Pickbyopen.Database
{
    public class PartnumberRepository(IDbConnectionFactory connectionFactory)
        : IPartnumberRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // <summary>
        // Insert a new partnumber into the partnumbers table
        // </summary>
        public async Task InsertOrUpdatePartnumber(
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
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Não foi possível cadastrar o partnumber." + e);
                throw;
            }
        }

        // <summary>
        // Create an association between a partnumber and a door
        // </summary>
        public async Task<bool> CreateAssociation(string partnumber, string door)
        {
            try
            {
                if (string.IsNullOrEmpty(partnumber))
                    throw new ArgumentNullException(nameof(partnumber));

                if (string.IsNullOrEmpty(door))
                    throw new ArgumentNullException(nameof(door));

                using var connection = _connectionFactory.GetConnection();
                await connection.OpenAsync();

                await InsertOrUpdateAssociation(connection, partnumber, door);
                return true;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Não foi possível criar a associação." + e);
                throw;
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
                ErrorMessage.Show("Não foi possível criar ou atualizar a associação." + e);
                throw;
            }
        }

        // <summary>
        // Delete an association between a partnumber and a door
        // </summary>
        public async Task<bool> DeletePartnumberIndex(string partnumber)
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM public.partnumbers_index WHERE partnumber = @partnumber;",
                    connection
                );
                deleteCommand.Parameters.AddWithValue("@partnumber", partnumber);
                await deleteCommand.ExecuteNonQueryAsync();
                return true;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Erro ao deletar a associação." + e);
                throw;
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

            using var connection = _connectionFactory.GetConnection();
            await connection.OpenAsync();

            await InsertOrUpdatePartnumber(connection, partnumber, description);

            if (!string.IsNullOrEmpty(door))
                await InsertOrUpdateAssociation(connection, partnumber, door);

            return true;
        }

        public ObservableCollection<Partnumber> LoadPartnumberList()
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
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
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM public.partnumbers WHERE partnumber = @partnumber;",
                    connection
                );
                deleteCommand.Parameters.AddWithValue("@partnumber", partnumber);
                await deleteCommand.ExecuteNonQueryAsync();

                return true;
            }
            catch (PostgresException)
            {
                ErrorMessage.Show(
                    "Não foi possível deletar o partnumber. Verifique se o partnumber está associado à alguma porta e desfaça a associação."
                );
                return false;
                throw;
            }
        }

        // <summary>
        // Get the door associated with a partnumber
        // </summary>
        public async Task<int> GetAssociatedDoor(string partnumber)
        {
            try
            {
                var connection = _connectionFactory.GetConnection();
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
                ErrorMessage.Show("Erro ao buscar a associação." + e);
                throw;
            }
        }

        // <summary>
        // Load a list of partnumbers without associations
        // </summary>
        public async Task<ObservableCollection<string>> LoadAvailablePartnumbers()
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                DataTable dt = new();

                ObservableCollection<string> ptnList = [];
                using var command = new NpgsqlCommand(
                    "SELECT partnumber FROM public.partnumbers WHERE partnumber NOT IN(SELECT partnumber FROM public.partnumbers_index);",
                    connection
                );
                using var reader = await command.ExecuteReaderAsync();
                dt.Load(reader);

                foreach (DataRow row in dt.Rows)
                {
                    ptnList.Add(item: row[0].ToString()!);
                }

                return ptnList;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Erro ao carregar a lista de partnumbers." + e);
                throw;
            }
        }

        // <summary>
        // Load a list of partnumbers associated with a door
        // </summary>
        public async Task<ObservableCollection<string>> LoadAssociatedPartnumbers(string door)
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                DataTable dt = new();

                ObservableCollection<string> ptnList = [];

                using var command = new NpgsqlCommand(
                    "SELECT partnumber FROM public.partnumbers_index WHERE door = @door;",
                    connection
                );
                command.Parameters.AddWithValue("@door", door);
                using var reader = await command.ExecuteReaderAsync();

                dt.Load(reader);

                foreach (DataRow row in dt.Rows)
                {
                    ptnList.Add(item: row[0].ToString()!);
                }

                return ptnList;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Erro ao carregar a lista de partnumbers." + e);
                throw;
            }
        }
    }
}
