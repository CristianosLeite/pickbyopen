﻿using Pickbyopen.Models;

namespace Pickbyopen.Interfaces
{
    public interface ILogRepository
    {
        Task SaveLog(Log log);
        Task<List<Log>> LoadLogs();
        Task LogUserLogin(User user);
        Task LogUserLogout(User user);
        Task LogUserOperate(string context, string target, string door, string mode);
        Task LogUserEditPartnumber(User user, string partnumber, string context);
        Task LogUserDeletePartnumber(User user, string partnumber);
        Task LogUserEditUser(User user, string target, string context);
        Task LogUserDeleteUser(User user, string target);
        Task LogSysSwitchedMode(string mode);
        Task LogSysPlcStatusChanged(string status);
    }
}
