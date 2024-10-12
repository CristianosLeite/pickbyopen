using System.Diagnostics;
using PCSC;
using PCSC.Monitoring;

namespace Pickbyopen.Devices.Nfc
{
    public class ACR122U
    {
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

        public void Init(bool buzzerOnOff)
        {
            this.buzzerOnOff = buzzerOnOff;

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

        private static void SetBuzzer(ICardReader reader, bool on)
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
