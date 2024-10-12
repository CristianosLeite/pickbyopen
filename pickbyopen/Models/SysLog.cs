namespace Pickbyopen.Models
{
    public class SysLog(DateTime createdAt, string @event, string @target, string device)
        : Log(createdAt, @event, @target)
    {
        public string Device { get; set; } = device;
    }
}
