using Pickbyopen.Settings;
using Sharp7.Rx;
using Sharp7.Rx.Enums;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace Pickbyopen.Devices.Plc
{
    public class Plc
    {
        private readonly Sharp7Plc Client;
        private readonly string Ip = SPlc.Default.Ip;
        private readonly int Rack = SPlc.Default.Rack;
        private readonly int Slot = SPlc.Default.Slot;

        public Plc()
        {
            Client = new Sharp7Plc(Ip, Rack, Slot);
            Task.Run(async () =>
            {
                await InitializePlc();
            });
        }

        public async Task<bool> InitializePlc()
        {
            try
            {
                return await Connect();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao conectar com o PLC. " + ex.Message);
            }
        }

        public async Task<bool> Connect()
        {
            try
            {
                await Client.InitializeConnection();
                Thread.Sleep(1000);
                return await GetPlcStatus();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao conectar com o PLC." + ex);
            }
        }

        public async Task<bool> GetPlcStatus()
        {
            try
            {
                var connectionStatus = await ReadFromPlc(SPlcAddresses.Default.ReadPlcStatus);
                if ((bool)connectionStatus)
                    return true;
            }
            catch
            {
                return false;
            }
            return false;
        }

        public IDisposable SubscribeAddress<T>(string address, Action<T> callback)
        {
            return Client
                .CreateNotification<T>(address, TransmissionMode.OnChange)
                .Subscribe(callback);
        }

        public async Task HandleFloats(string tag, float value)
        {
            await Client.SetValue(tag, value);
        }

        public async Task HandleNumbers(string tag, int value)
        {
            if (tag.Contains("DINT"))
            {
                await Client.SetValue(tag, value * 1000);
            }
            else
            {
                await Client.SetValue(tag, value);
            }
        }

        public async Task<object> ReadFromPlc(string tag)
        {
            try
            {
                return await Client.GetValue(tag);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler a tag {tag} do PLC. " + ex.Message);
            }
        }

        public async Task WriteToPlc(string tag, string value)
        {
            try
            {
                await HandleValue(tag, value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao escrever a tag {tag} no PLC. " + ex.Message);
            }
        }

        private async Task HandleValue(string tag, string value)
        {
            value = value.Replace(".", ",");

            if (tag.Contains("DINT") || tag.Contains("INT"))
            {
                await HandleNumbers(tag, int.Parse(value));
            }
            else if (Regex.IsMatch(tag, @"D[0-9]"))
            {
                await HandleFloats(tag, (float)double.Parse(value));
            }
            else if (tag.Contains("STRING"))
            {
                await Client.SetValue(tag, value);
            }
            else if (tag.Contains("DBX"))
            {
                await Client.SetValue(tag, bool.Parse(value));
            }
            else if (tag.Contains("BYTE"))
            {
                await Client.SetValue(tag, byte.Parse(value));
            }
            else
            {
                throw new Exception("Parâmetro incorreto para tag");
            }
        }
    }
}
