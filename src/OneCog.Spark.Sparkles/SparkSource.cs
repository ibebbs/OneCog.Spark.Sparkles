using OneCog.Io.Spark;
using OneCog.Spark.Sparkles.Document;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

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

        private IObservable<IDocument> HandleAndResubscribe(Configuration.IVariable variable, Exception exception, IObservable<IDocument> variableObservable)
        {
            Instrumentation.SparkCore.ErrorWhileObservingVariable(variable, exception);

            return variableObservable;
        }

        private IObservable<IDocument> HandleAndResubscribe(Exception exception, IObservable<IDocument> variableObservable)
        {
            Instrumentation.SparkCore.ErrorWhileObserving(exception);

            return variableObservable;
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

            return observable.Catch<IDocument, Exception>(exception => HandleAndResubscribe(variable, exception, observable)).Repeat();
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

            return observable.Catch<IDocument, Exception>(exception => HandleAndResubscribe(exception, observable)).Repeat();
        }

        public IDisposable Subscribe(IObserver<IDocument> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
