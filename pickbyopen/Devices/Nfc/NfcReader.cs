using PCSC;
using PCSC.Monitoring;
using System.Diagnostics;

namespace Pickbyopen.Devices.Nfc
{
    public class ACR122U
    {
        private int maxReadWriteLength = 50;
        private int blockSize = 4;
        private int startBlock = 4;
        private int readbackDelayMilliseconds = 100;
        private string[]? cardReaderNames;
        private ISCardContext? cardContext;
        private bool buzzerSet = false;
        private bool buzzerOnOff = false;

        public event CardInsertedHandler CardInserted;
        public delegate void CardInsertedHandler(ICardReader reader);

        public event CardRemovedHandler CardRemoved;
        public delegate void CardRemovedHandler();

        public ACR122U()
        {
            CardInserted = delegate { };
            CardRemoved = delegate { };
        }

        public void Init(
            bool buzzerOnOff,
            int maxReadWriteLength,
            int blockSize,
            int startBlock,
            int readbackDelayMilliseconds
        )
        {
            this.buzzerOnOff = buzzerOnOff;
            this.maxReadWriteLength = maxReadWriteLength;
            this.blockSize = blockSize;
            this.startBlock = startBlock;
            this.readbackDelayMilliseconds = readbackDelayMilliseconds;

            cardContext = ContextFactory.Instance.Establish(SCardScope.System);

            cardReaderNames = cardContext.GetReaders();

            var monitor = MonitorFactory.Instance.Create(SCardScope.System);
            monitor.CardInserted += Monitor_CardInserted;
            monitor.CardRemoved += Monitor_CardRemoved;
            monitor.Start(cardReaderNames);
        }

        private void Monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            if (cardContext == null || cardReaderNames == null || cardReaderNames.Length == 0)
            {
                return;
            }

            ICardReader? reader = null;

            try
            {
                reader = cardContext.ConnectReader(
                    cardReaderNames[0],
                    SCardShareMode.Shared,
                    SCardProtocol.Any
                );
            }
            catch { }

            if (reader != null)
            {
                if (!buzzerSet)
                {
                    buzzerSet = true;
                    SetBuzzer(reader, buzzerOnOff);
                }

                CardInserted?.Invoke(reader);

                try
                {
                    reader.Disconnect(SCardReaderDisposition.Leave);
                }
                catch { }
            }
        }

        private void Monitor_CardRemoved(object sender, CardStatusEventArgs e)
        {
            CardRemoved?.Invoke();
        }

        public static byte[]? GetUID(ICardReader reader)
        {
            byte[] uid = new byte[10];

            try
            {
                reader.Transmit([0xFF, 0xCA, 0x00, 0x00, 0x00], uid);
                Array.Resize(ref uid, 7);
            }
            catch (PCSC.Exceptions.RemovedCardException ex)
            {
                // Handle the exception (e.g., log the error, notify the user)
                Debug.WriteLine("Card was removed: " + ex.Message);
                return null;
            }
            catch (PCSC.Exceptions.ReaderUnavailableException ex)
            {
                // Handle the exception (e.g., log the error, notify the user)
                Debug.WriteLine("Card reader is unavailable: " + ex.Message);
                return null;
            }

            return uid;
        }

        public static bool CheckAvailaility(ICardReader reader)
        {
            return reader.CardHandle.IsConnected && reader.CardHandle.Handle != nint.Zero;
        }

        public static bool Authenticate(ICardReader reader, int block, byte keyType, byte[] key)
        {
            byte[] command = [0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, (byte)block, keyType, 0x00];
            byte[] response = new byte[2];

            try
            {
                reader.Transmit(command, response);

                if (response[0] == 0x90 && response[1] == 0x00)
                {
                    return true;
                }
                else
                {
                    Debug.WriteLine("Authentication failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"An unexpected error occurred during authentication: {ex.Message}"
                );
                return false;
            }
        }

        public static byte[]? Read(ICardReader reader, int block, int len, byte keyType, byte[] key)
        {
            if (!Authenticate(reader, block, keyType, key))
            {
                return null;
            }

            byte[] data = new byte[len + 2]; // Buffer para dados + status words
            byte[] command = [0xFF, 0x00, 0x00, (byte)block, (byte)len];

            try
            {
                if (!CheckAvailaility(reader))
                    return null;
                reader.Transmit(command, data);

                if (data[len] == 0x63 && data[len + 1] == 0x00)
                {
                    Debug.WriteLine("Warning: Operation failed with status 63 00.");
                    return null;
                }
            }
            catch (PCSC.Exceptions.ReaderUnavailableException)
            {
                Debug.WriteLine(
                    "The specified reader is not currently available for use. Please check the connection and try again."
                );
                return null;
            }
            catch (PCSC.Exceptions.RemovedCardException)
            {
                Debug.WriteLine("The smart card has been removed. Please reinsert the card.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error occurred: {ex.Message}");
                return null;
            }

            Array.Resize(ref data, len);
            return data;
        }

        public static void Write(ICardReader reader, int block, int len, byte[] data)
        {
            byte[] ret = new byte[2];
            List<byte> cmd = [0xFF, 0xD6, 0x00, (byte)block, (byte)len];
            cmd.AddRange(data);

            reader.Transmit([.. cmd], ret);
        }

        public bool WriteData(ICardReader reader, byte[] data)
        {
            Array.Resize(ref data, maxReadWriteLength);

            int pos = 0;
            while (pos < data.Length)
            {
                byte[] buf = new byte[blockSize];
                int len = data.Length - pos > blockSize ? blockSize : data.Length - pos;
                Array.Copy(data, pos, buf, 0, len);

                Write(reader, pos / blockSize + startBlock, blockSize, buf);

                pos += blockSize;
            }

            Thread.Sleep(readbackDelayMilliseconds);

            byte[] readback = ReadData(reader);

            return data.SequenceEqual(readback);
        }

        public byte[] ReadData(ICardReader reader)
        {
            List<byte> data = [];

            int pos = 0;
            while (pos < maxReadWriteLength)
            {
                int len =
                    maxReadWriteLength - pos > blockSize ? blockSize : maxReadWriteLength - pos;

                byte[]? buf = Read(
                    reader,
                    pos / blockSize + startBlock,
                    len,
                    BitConverter.GetBytes(0x60)[0],
                    [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]
                );

                if (buf != null)
                {
                    data.AddRange(buf);
                }
                else
                {
                    Console.WriteLine("Failed to read data from card.");
                    break;
                }

                pos += blockSize;
            }

            // Remove padding or extra bytes if necessary
            return data.Take(maxReadWriteLength).ToArray();
        }

        public static void SetBuzzer(ICardReader reader, bool on)
        {
            try
            {
                byte[] ret = new byte[2];

                reader.Transmit([0xFF, 0x00, 0x52, (byte)(on ? 0xff : 0x00), 0x00], ret);
            }
            catch
            {
                Debug.WriteLine("Failed to set buzzer state.");
            }
        }
    }
}
