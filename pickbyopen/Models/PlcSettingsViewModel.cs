using CommunityToolkit.Mvvm.Input;
using Pickbyopen.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Pickbyopen.Models
{
    public partial class PlcSettingsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SettingsModel> PlcConfigurations { get; set; }
        public ICommand SaveCommand { get; }

        public PlcSettingsViewModel()
        {
            PlcConfigurations =
            [
                new SettingsModel { Name = "Ip", Value = SPlc.Default.Ip },
                new SettingsModel { Name = "Rack", Value = SPlc.Default.Rack.ToString() },
                new SettingsModel { Name = "Slot", Value = SPlc.Default.Slot.ToString() },
                new SettingsModel
                {
                    Name = "ReadIsOpen1",
                    Value = SPlcAddresses.Default.ReadIsOpen1,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen2",
                    Value = SPlcAddresses.Default.ReadIsOpen2,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen3",
                    Value = SPlcAddresses.Default.ReadIsOpen3,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen4",
                    Value = SPlcAddresses.Default.ReadIsOpen4,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen5",
                    Value = SPlcAddresses.Default.ReadIsOpen5,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen6",
                    Value = SPlcAddresses.Default.ReadIsOpen6,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen7",
                    Value = SPlcAddresses.Default.ReadIsOpen7,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen8",
                    Value = SPlcAddresses.Default.ReadIsOpen8,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen9",
                    Value = SPlcAddresses.Default.ReadIsOpen9,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen10",
                    Value = SPlcAddresses.Default.ReadIsOpen10,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen11",
                    Value = SPlcAddresses.Default.ReadIsOpen11,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen12",
                    Value = SPlcAddresses.Default.ReadIsOpen12,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen13",
                    Value = SPlcAddresses.Default.ReadIsOpen13,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen14",
                    Value = SPlcAddresses.Default.ReadIsOpen14,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen15",
                    Value = SPlcAddresses.Default.ReadIsOpen15,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen16",
                    Value = SPlcAddresses.Default.ReadIsOpen16,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen17",
                    Value = SPlcAddresses.Default.ReadIsOpen17,
                },
                new SettingsModel
                {
                    Name = "ReadIsOpen18",
                    Value = SPlcAddresses.Default.ReadIsOpen18,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill10",
                    Value = SPlcAddresses.Default.ReadNeedsRefill10,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill11",
                    Value = SPlcAddresses.Default.ReadNeedsRefill11,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill12",
                    Value = SPlcAddresses.Default.ReadNeedsRefill12,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill13",
                    Value = SPlcAddresses.Default.ReadNeedsRefill13,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill14",
                    Value = SPlcAddresses.Default.ReadNeedsRefill14,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill15",
                    Value = SPlcAddresses.Default.ReadNeedsRefill15,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill16",
                    Value = SPlcAddresses.Default.ReadNeedsRefill16,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill17",
                    Value = SPlcAddresses.Default.ReadNeedsRefill17,
                },
                new SettingsModel
                {
                    Name = "ReadNeedsRefill18",
                    Value = SPlcAddresses.Default.ReadNeedsRefill18,
                },
                new SettingsModel
                {
                    Name = "ReadPlcStatus",
                    Value = SPlcAddresses.Default.ReadPlcStatus,
                },
                new SettingsModel
                {
                    Name = "ReadDoorIsOpen",
                    Value = SPlcAddresses.Default.ReadDoorIsOpen,
                },
                new SettingsModel
                {
                    Name = "WriteFrontsideDoor",
                    Value = SPlcAddresses.Default.WriteFrontsideDoor,
                },
                new SettingsModel
                {
                    Name = "WriteBacksideDoor",
                    Value = SPlcAddresses.Default.WriteBacksideDoor,
                },
            ];

            SaveCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            foreach (var config in PlcConfigurations)
            {
                if (config.Name.StartsWith("Read") || config.Name.StartsWith("Write"))
                {
                    SPlcAddresses.Default[config.Name] = config.Value;
                }
                else if (config.Name == "Rack" || config.Name == "Slot")
                {
                    string name = config.Name;
                    SPlc.Default[name] = int.Parse(config.Value);
                }
                else if (
                    config.Name == "Rack"
                    || config.Name == "Slot"
                    || config.Name == "ReadPlcStatus"
                    || config.Name == "ReadDoorIsOpen"
                    || config.Name == "WriteFrontsideDoor"
                    || config.Name == "WriteBacksideDoor"
                )
                {
                    SPlc.Default[config.Name] = config.Value;
                }
            }
            SPlc.Default.Save();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
