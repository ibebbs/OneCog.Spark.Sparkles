using OneCog.Io.Spark;
using OneCog.Spark.Sparkles.Document;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
            return Observable.Create<IDocument>(
                observer =>
                {
                    TimeSpan interval = variable.Interval ?? device.DefaultInterval ?? settings.DefaultInterval;
                    string type = variable.Type ?? device.DefaultType ?? settings.DefaultType;
                    string indexName = variable.IndexName ?? device.DefaultIndexName ?? settings.DefaultIndexName;

                    IConnectableObservable<Fallible<IVariable>> observable = _sparkApi
                        .ObserveVariable(device.Id, variable.Name, interval)
                        .Timeout(TimeSpan.FromTicks(interval.Ticks * 5), schedulerProvider.AsyncScheduler)
                        .Retry(exception => Instrumentation.SparkCore.UnhandledErrorWhileObservingVariable(variable, exception))
                        .Publish();

                    IDisposable documentSubscription = observable
                        .Where(fallible => fallible.HasValue)
                        .Select(sparkVariable => documentFactory.CreateDocument(sparkVariable.Value, indexName, type))
                        .Subscribe(observer);

                    IDisposable failedSubscription = observable
                        .Where(fallible => fallible.HasFailed)
                        .Subscribe(fallible => Instrumentation.SparkCore.HandledErrorWhileObservingVariable(variable, fallible.Exception));

                    return new CompositeDisposable(
                        documentSubscription,
                        failedSubscription,
                        observable.Connect()
                    );
                }
            );
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
