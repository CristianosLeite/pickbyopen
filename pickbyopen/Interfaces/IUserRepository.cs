using Pickbyopen.Models;
using Pickbyopen.Types;
using System.Collections.ObjectModel;

namespace Pickbyopen.Interfaces
{
    public interface IUserRepository
    {
        ObservableCollection<User> LoadUsersList();
        Task<User?> GetUserById(string id);
        Task<bool> SaveUser(User user, Context context);
        Task<User?> FindUserByBadgeNumber(string badgeNumber);
        Task<bool> DeleteUser(User user);
    }
}
