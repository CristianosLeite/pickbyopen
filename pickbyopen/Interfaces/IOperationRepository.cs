using Pickbyopen.Models;
using System.Collections.ObjectModel;

namespace Pickbyopen.Interfaces
{
    public interface IOperationRepository
    {
        Task<ObservableCollection<Operation>> LoadOperations();
        Task<List<Operation>> GetOperationsByDate(string vpOrPartnumber, string chassi, string door, string initialDate, string finalDate);
    }
}
