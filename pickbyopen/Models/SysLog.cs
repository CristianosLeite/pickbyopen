using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pickbyopen.Models
{
    public class SysLog(DateTime createdAt, string @event, string @target, string device)
        : Log(createdAt, @event, @target)
    {
        public string Device { get; set; } = device;
    }
}
