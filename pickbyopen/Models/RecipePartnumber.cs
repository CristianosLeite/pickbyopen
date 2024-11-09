namespace Pickbyopen.Models
{
    public class RecipePartnumber
    {
        public long RecipeId { get; set; }
        public string VP { get; set; } = string.Empty;
        public long PartnumberId { get; set; }
        public Recipe? Recipe { get; set; }
        public Partnumber? Partnumber { get; set; }
    }
}
