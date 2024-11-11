﻿using Pickbyopen.Devices.Plc;
using Pickbyopen.Interfaces;
using Pickbyopen.Settings;
using Pickbyopen.Types;

namespace Pickbyopen.Services
{
    public class PlcService(Plc plc, ModeService modeService, LogService logService) : IPlcService
    {
        private readonly Plc _plc = plc;
        private readonly ModeService _modeService = modeService;
        private readonly LogService _logService = logService;

        public async Task<bool> Connect() => await _plc.Connect();

        public async Task<bool> EnsureConnection() => await _plc.GetPlcStatus();

        public async Task WriteToPlc(int door, string? target, string? chassi, Event @event)
        {
            if (door < 10)
                await _plc.WriteToPlc(SPlcAddresses.Default.WriteFrontsideDoor, door.ToString());
            else
                await _plc.WriteToPlc(SPlcAddresses.Default.WriteBacksideDoor, door.ToString());

            if (!_modeService.IsMaintenance)
            {
                await _logService.LogUserOperate(
                    @event == Event.Reading ? "Leitura" : "Seleção",
                    target ?? string.Empty,
                    chassi ?? string.Empty,
                    door.ToString(),
                    _modeService.IsAutomatic ? "Automático" : "Manual",
                    Auth.GetUserId()
                );
            }
        }
    }
}
