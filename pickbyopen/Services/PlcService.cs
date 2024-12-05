using System.Diagnostics;
using Pickbyopen.Devices.Plc;
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
            switch (door)
            {
                case 1:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen1, "true");
                    break;
                case 2:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen2, "true");
                    break;
                case 3:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen3, "true");
                    break;
                case 4:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen4, "true");
                    break;
                case 5:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen5, "true");
                    break;
                case 6:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen6, "true");
                    break;
                case 7:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen7, "true");
                    break;
                case 8:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen8, "true");
                    break;
                case 9:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen9, "true");
                    break;
                case 10:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen10, "true");
                    break;
                case 11:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen11, "true");
                    break;
                case 12:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen12, "true");
                    break;
                case 13:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen13, "true");
                    break;
                case 14:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen14, "true");
                    break;
                case 15:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen15, "true");
                    break;
                case 16:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen16, "true");
                    break;
                case 17:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen17, "true");
                    break;
                case 18:
                    await _plc.WriteToPlc(SPlcAddresses.Default.WriteOpen18, "true");
                    break;
                default:
                    Debug.WriteLine("Door not found.");
                    break;
            }

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
