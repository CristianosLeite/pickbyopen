using System.IO.Ports;

namespace Pickbyopen.Devices.CodebarsReader
{
    internal class CodebarsReader
    {
        SerialPort port;
        string data;

        public CodebarsReader()
        {
            port = new();
            data = "";
        }

        private static bool CheckPort(string COM)
        {
            string[] ports = SerialPort.GetPortNames();
            return ports.Contains(COM);
        }

        public void Connect(string COM)
        {
            try
            {
                if (!CheckPort(COM))
                {
                    throw new Exception($"Porta {COM} não está disponível.");
                }

                port = new SerialPort(COM);
                port.DataReceived += Port_DataReceived;
                port.Open();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(Exception))
                {
                    throw new Exception($"Erro ao conectar na porta {COM}: {e.Message}");
                }
            }
        }

        public string GetData()
        {
            return data;
        }

        public void ClearComPort()
        {
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            data = "";
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            data = port.ReadExisting();
        }

        public void Close()
        {
            port.Close();
        }
    }
}
