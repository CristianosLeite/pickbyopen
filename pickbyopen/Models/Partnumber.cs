namespace Pickbyopen.Models
{
    public class Partnumber(string partnumber, string description)
    {
        public string Code { get; set; } = partnumber;
        public string Description { get; set; } = description;
    }
}
