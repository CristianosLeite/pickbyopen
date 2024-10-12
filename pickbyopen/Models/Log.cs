﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pickbyopen.Models
{
    public abstract class Log(DateTime createdAt, string @event, string @target)
    {
        public DateTime CreatedAt { get; set; } = createdAt;
        public string Event { get; set; } = @event;
        public string Target { get; set; } = @target;
    }
}