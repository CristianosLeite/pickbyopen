using Pickbyopen.Types;

namespace Pickbyopen.Interfaces
{
    public interface IPlcService
    {
        Task<bool> Connect();
        Task<bool> EnsureConnection();
        Task WriteToPlc(int door, string target, string van, Event @event);
    }
}
