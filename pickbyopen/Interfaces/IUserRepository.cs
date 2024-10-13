using Pickbyopen.Models;
using System.Collections.ObjectModel;

namespace Pickbyopen.Interfaces
{
    public interface IUserRepository
    {
        ObservableCollection<User> LoadUsersList();
        Task<User?> GetUserById(string id);
        Task<bool> SaveUser(User user, string context);
        Task<User?> FindUserByBadgeNumber(string badgeNumber);
        Task<bool> DeleteUser(User user);
    }
}
