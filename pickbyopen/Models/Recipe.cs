namespace Pickbyopen.Models
{
    public class Recipe(long? recipeId, string vp, string description)
    {
        public long? RecipeId { get; set; } = recipeId;
        public string VP { get; set; } = vp;
        public string Description { get; set; } = description;
        public List<Partnumber> Partnumbers { get; set; } = [];
        public List<RecipePartnumber> RecipePartnumbers { get; set; } = [];
    }
}
