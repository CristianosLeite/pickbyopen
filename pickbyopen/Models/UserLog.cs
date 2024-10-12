using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pickbyopen.Models
{
    public class UserLog(DateTime createdAt, string @event, string @target, User user)
        : Log(createdAt, @event, @target)
    {
        public User User { get; set; } = user;
    }
}
