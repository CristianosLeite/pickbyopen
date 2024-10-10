namespace Pickbyopen.Models
{
    public class User(string id, string badgeNumber, string username, List<string> permissions)
    {
        public string Id { get; set; } = id;
        public string BadgeNumber { get; set; } = badgeNumber;
        public string Username { get; set; } = username;
        public List<string> Permissions { get; set; } = permissions;
        public string StringPermissions => string.Join(", ", Permissions);

        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}
