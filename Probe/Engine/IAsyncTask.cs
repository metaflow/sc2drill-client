namespace Probe.Engine
{
    public interface IAsyncTask
    {
        void Run();
        void OnComplete();
        bool Success { get; }
        AsyncTaskProcessor Processor { get; set; }
    }
}
