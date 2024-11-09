using Pickbyopen.Models;
using Pickbyopen.Types;
using System.Collections.ObjectModel;

namespace Pickbyopen.Interfaces
{
    public interface IRecipeRepository
    {
        ObservableCollection<Recipe> LoadRecipeList();
        Task<Recipe?> GetRecipeByVp(string vp);
        Task<List<int>> GetRecipeAssociatedDoors(string vp);
        Task<bool> SaveRecipe(Recipe recipe, List<Partnumber> partnumbers, Context context);
        Task<bool> CreateRecipePartnumberAssociation(string vp, long partnumberId, long recipeId);
        Task<bool> DeleteRecipePartnumberAssociation(string vp);
        Task<bool> DeleteRecipe(string vp);
    }
}
