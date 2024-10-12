using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Windows;

namespace Pickbyopen.Services
{
    public static class Auth
    {
        private static readonly Db db = new(DatabaseConfig.ConnectionString!);
        public static User? LoggedInUser { get; private set; }
        public static string? LoggedAt { get; private set; }

        public async static void SetLoggedInUser(User user)
        {
            LoggedInUser = user;
            await db.LogUserLogin(user);
        }

        public static void SetLoggedAt(string time)
        {
            LoggedAt = time;
        }

        public static bool UserHasPermission(string permission)
        {
            return LoggedInUser?.HasPermission(permission) ?? false;
        }

        public async static void Logout()
        {
            await db.LogUserLogout(LoggedInUser!);
            LoggedInUser = null;
            LoggedAt = null;

            NfcWindow nfcWindow = new("login");
            nfcWindow.ShowDialog();
        }
    }
}
