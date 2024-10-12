using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pickbyopen.Models
{
    public class Operation(DateTime createdAt, string @event, string target, string door, string mode) : Log(createdAt, @event, target)
    {
        public string Door { get; set; } = door;
        public string Mode { get; set; } = mode;
    }
}
