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
        Task<ObservableCollection<Partnumber>> LoadPartnumberList();
        Task<bool> DeletePartnumber(string partnumber);
        Task<bool> DeletePartnumberIndex(string partnumber);
        Task<bool> CreateAssociation(string partnumber, string door);
        Task<int> GetAssociatedDoor(string partnumber);
        Task<ObservableCollection<string>> LoadAvailablePartnumbers();
        Task<ObservableCollection<string>> LoadAssociatedPartnumbers(string door);
    }
}
