namespace Pickbyopen.Interfaces
{
    public interface ICodeBarsReaderService
    {
        void InitializeCodeBarsReader();
        void SubscribeReader(EventHandler<string> handler);
    }
}
