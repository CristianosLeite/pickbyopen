namespace Pickbyopen.Models
{
    public class Partnumber(long id, string partnumber, string description)
    {
        public long PartnumberId { get; set; } = id;
        public string Code { get; set; } = partnumber;
        public string Description { get; set; } = description;
        public List<RecipePartnumber> RecipePartnumbers { get; set; } = [];
    }
}
