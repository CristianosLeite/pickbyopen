using System.Collections.ObjectModel;
using Npgsql;
using Pickbyopen.Interfaces;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Utils;

namespace Pickbyopen.Database
{
    internal class RecipeRepository(IDbConnectionFactory connectionFactory) : IRecipeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        public ObservableCollection<Recipe> LoadRecipeList()
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                var recipeList = new ObservableCollection<Recipe>();

                using var command = new NpgsqlCommand(
                    "SELECT recipe_id, vp, description FROM public.recipes;",
                    connection
                );
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var recipe = new Recipe(
                        reader.GetInt32(0), // RecipeId
                        reader.GetString(1), // VP
                        reader.GetString(2) // Description
                    )
                    {
                        RecipePartnumbers = [],
                    };

                    // Carregar associações de Partnumber
                    recipe.RecipePartnumbers = LoadRecipePartnumbers(reader.GetString(1));

                    Db db = new(_connectionFactory);

                    recipe.Partnumbers.AddRange(
                        db.LoadPartnumberList()
                            .Where(p =>
                                recipe.RecipePartnumbers.Any(rp =>
                                    rp.PartnumberId == p.PartnumberId
                                )
                            )
                    );

                    recipeList.Add(recipe);
                }

                return recipeList;
            }
            catch (PostgresException e)
            {
                throw new Exception("Erro ao carregar a lista de receitas." + e);
            }
        }

        private List<RecipePartnumber> LoadRecipePartnumbers(string vp)
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                var recipePartnumbers = new List<RecipePartnumber>();

                using var command = new NpgsqlCommand(
                    "SELECT recipe_id, vp, partnumber_id FROM public.recipepartnumber WHERE vp = @vp;",
                    connection
                );
                command.Parameters.AddWithValue("@vp", vp);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var recipePartnumber = new RecipePartnumber
                    {
                        RecipeId = reader.GetInt32(0),
                        VP = reader.GetString(1),
                        PartnumberId = reader.GetInt32(2),
                    };

                    recipePartnumbers.Add(recipePartnumber);
                }

                return recipePartnumbers;
            }
            catch (PostgresException e)
            {
                throw new Exception("Erro ao carregar a lista de associações de receitas." + e);
            }
        }

        public async Task<bool> SaveRecipe(
            Recipe recipe,
            List<Partnumber> partnumbers,
            Context context
        )
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    recipe.RecipeId == null
                        ? "INSERT INTO public.recipes (vp, description) VALUES (@vp, @description) RETURNING recipe_id;"
                        : "UPDATE public.recipes SET vp = @vp, description = @description WHERE recipe_id = @recipeId;",
                    connection
                );

                if (recipe.RecipeId != null)
                    command.Parameters.AddWithValue("@recipeId", recipe.RecipeId);

                command.Parameters.AddWithValue("@vp", recipe.VP);
                command.Parameters.AddWithValue("@description", recipe.Description);

                long recipeId;
                if (recipe.RecipeId == null)
#pragma warning disable CS8605 // If the value is null, an exception will be thrown
                    recipeId = (long)await command.ExecuteScalarAsync();
#pragma warning restore CS8605
                else
                {
                    await command.ExecuteNonQueryAsync();
                    recipeId = recipe.RecipeId.Value;
                }

                if (context == Context.Update)
                    await DeleteRecipePartnumberAssociation(recipe.VP);

                foreach (var partnumber in partnumbers)
                    await CreateRecipePartnumberAssociation(
                        recipe.VP,
                        partnumber.PartnumberId,
                        recipeId
                    );

                return true;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Erro ao salvar a receita." + e);
                return false;
                throw;
            }
        }

        public async Task<bool> CreateRecipePartnumberAssociation(
            string vp,
            long partnumberId,
            long recipeId
        )
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    "INSERT INTO RecipePartnumber (vp, partnumber_id, recipe_id) VALUES (@vp, @partnumberId, @recipeId)",
                    connection
                );
                command.Parameters.AddWithValue("@vp", vp);
                command.Parameters.AddWithValue("@partnumberId", partnumberId);
                command.Parameters.AddWithValue("@recipeId", recipeId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Erro ao criar associação de receita." + e);
                return false;
                throw;
            }
        }

        public async Task<bool> DeleteRecipePartnumberAssociation(string vp)
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    "DELETE FROM RecipePartnumber WHERE vp = @vp",
                    connection
                );
                command.Parameters.AddWithValue("@vp", vp);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Erro ao deletar associação de receita." + e);
                return false;
                throw;
            }
        }

        public async Task<bool> DeleteRecipe(string vp)
        {
            try
            {
                using var connection = _connectionFactory.GetConnection();
                connection.Open();

                var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM public.recipes WHERE vp = @vp;",
                    connection
                );
                deleteCommand.Parameters.AddWithValue("@vp", vp);
                await deleteCommand.ExecuteNonQueryAsync();

                return true;
            }
            catch (PostgresException e)
            {
                ErrorMessage.Show("Não foi possível deletar a receita." + e);
                return false;
                throw;
            }
        }
    }
}
