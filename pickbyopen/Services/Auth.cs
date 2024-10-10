using Pickbyopen.Models;
using Pickbyopen.Windows;

namespace Pickbyopen.Services
{
    public static class Auth
    {
        public static User? LoggedInUser { get; private set; }
        public static string? LoggedAt { get; private set; }

        public static void SetLoggedInUser(User user)
        {
            LoggedInUser = user;
        }

        public static void SetLoggedAt(string time)
        {
            LoggedAt = time;
        }

        public static bool UserHasPermission(string permission)
        {
            return LoggedInUser?.HasPermission(permission) ?? false;
        }

        public static void Logout()
        {
            LoggedInUser = null;
            LoggedAt = null;

            NfcWindow nfcWindow = new("login");
            nfcWindow.ShowDialog();
        }
    }
}
