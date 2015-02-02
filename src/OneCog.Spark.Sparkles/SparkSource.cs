using OneCog.Io.Spark;
using OneCog.Spark.Sparkles.Document;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace OneCog.Spark.Sparkles
{
    public interface ISparkSource : IDisposable, IObservable<IDocument>
    {

    }

    internal class SparkSource : ISparkSource
    {
        private readonly IApi _sparkApi;

        private IObservable<IDocument> _observable;

        public SparkSource(Configuration.ISettings settings, IApi sparkApi, Document.IFactory documentFactory, ISchedulerProvider schedulerProvider)
        {
            _sparkApi = sparkApi;
            _observable = CreateObservable(settings.SparkCore, sparkApi, documentFactory, schedulerProvider);
        }

        public void Dispose()
        {
        }

        private IObservable<IDocument> ConstructVariableObservable(IApi sparkApi, Document.IFactory documentFactory, ISchedulerProvider schedulerProvider, Configuration.ISparkCore settings, Configuration.IDevice device, Configuration.IVariable variable)
        {
            TimeSpan interval = variable.Interval ?? device.DefaultInterval ?? settings.DefaultInterval;
            string type = variable.Type ?? device.DefaultType ?? settings.DefaultType;
            string indexName = variable.IndexName ?? device.DefaultIndexName ?? settings.DefaultIndexName;

            var observable = _sparkApi
                .ObserveVariable(device.Id, variable.Name, interval)
                .Select(sparkVariable => documentFactory.CreateDocument(sparkVariable, indexName, type))
                .Timeout(TimeSpan.FromTicks(interval.Ticks * 5), schedulerProvider.AsyncScheduler);

            return observable.Retry(exception => Instrumentation.SparkCore.ErrorWhileObservingVariable(variable, exception));
        }

        private IObservable<IDocument> CreateObservable(Configuration.ISparkCore settings, IApi sparkApi, Document.IFactory documentFactory, ISchedulerProvider schedulerProvider)
        {
            var observable = Observable.Merge(
                settings.Devices.SelectMany(
                    device => device.Variables.Select(
                        variable => ConstructVariableObservable(sparkApi, documentFactory, schedulerProvider, settings, device, variable)
                    )
                )
            );

            return observable.Retry(exception => Instrumentation.SparkCore.ErrorWhileObserving(exception));
        }

        public IDisposable Subscribe(IObserver<IDocument> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
