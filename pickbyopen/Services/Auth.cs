using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Windows;

namespace Pickbyopen.Services
{
    public static class Auth
    {
        private static readonly Db db;
        public static User? LoggedInUser { get; private set; }
        public static string? LoggedAt { get; private set; }

        static Auth()
        {
            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);
        }

        public async static void SetLoggedInUser(User user)
        {
            LoggedInUser = user;
            await db.LogUserLogin(user);
        }

        public static void SetLoggedAt(string time)
        {
            LoggedAt = time;
        }

        public static string GetUserId()
        {
            return LoggedInUser?.Id ?? "0";
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

            NfcWindow nfcWindow = new(Context.Login);
            nfcWindow.ShowDialog();
        }
    }
}
