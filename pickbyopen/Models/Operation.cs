﻿namespace Pickbyopen.Models
{
    public class Operation(DateTime createdAt, string @event, string target, string? van, string door, string mode, string userId, string? username = null) : Log(createdAt, @event, target)
    {
        public string? Van { get; set; } = van;
        public string Door { get; set; } = door;
        public string Mode { get; set; } = mode;
        public string UserId { get; set; } = userId;
        public string? UserName { get; set; } = username;
    }
}
