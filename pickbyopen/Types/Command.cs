namespace Pickbyopen.Types
{
    public class Command
    {
        public bool Open { get; set; }
        public bool Refill { get; set; }
        public bool Empty { get; set; }

        public void SetOpen()
        {
            Open = true;
        }

        public void SetRefill()
        {
            Refill = true;
        }

        public void SetEmpty()
        {
            Empty = true;
        }
    }
}
