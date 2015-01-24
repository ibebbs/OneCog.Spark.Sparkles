using System.Reactive.Concurrency;

namespace OneCog.Spark.Sparkles
{
    public interface ISchedulerProvider
    {
        IScheduler AsyncScheduler { get; }
    }

    internal class SchedulerProvider : ISchedulerProvider
    {
        public IScheduler AsyncScheduler
        {
            get { return TaskPoolScheduler.Default; }
        }
    }
}
