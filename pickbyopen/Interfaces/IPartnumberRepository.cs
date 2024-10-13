using Npgsql;
using Pickbyopen.Models;
using System.Collections.ObjectModel;

namespace Pickbyopen.Interfaces
{
    public interface IPartnumberRepository
    {
        Task InsertOrUpdatePartnumber(
            NpgsqlConnection connection,
            string partnumber,
            string description
        );
        Task<bool> SavePartnumber(string partnumber, string description, string door);
        ObservableCollection<Partnumber> LoadPartnumberList();
        Task<bool> DeletePartnumber(string partnumber);
        Task<bool> DeletePartnumberIndex(string partnumber);
        Task<bool> CreateAssociation(string partnumber, string door);
        Task<int> GetAssociatedDoor(string partnumber);
        ObservableCollection<string> LoadAvailablePartnumbers();
        ObservableCollection<string> LoadAssociatedPartnumbers(string door);
    }
}
