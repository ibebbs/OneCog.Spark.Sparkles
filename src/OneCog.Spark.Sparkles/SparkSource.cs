using OneCog.Io.Spark;
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

        public SparkSource(Configuration.ISettings settings, IApi sparkApi, IDocumentFactory documentFactory)
        {
            _sparkApi = sparkApi;
            _observable = CreateObservable(settings, sparkApi, documentFactory);
        }

        private IObservable<IVariable> HandleAndResubscribe(Configuration.IVariable variable, Exception exception, IObservable<IVariable> variableObservable)
        {
            Instrumentation.SparkCore.ErrorWhileObservingVariable(variable, exception);

            return variableObservable;
        }

        private IObservable<IDocument> CreateObservable(Configuration.ISettings settings, IApi sparkApi, IDocumentFactory documentFactory)
        {
            var variables = Observable.Merge(
                settings.SparkCore.Devices.SelectMany(
                    device => device.Variables.Select(variable => ConstructVariableObservable(sparkApi, documentFactory, settings, device, variable))
                )
            );

            return variables;
        }

        private IObservable<IDocument> ConstructVariableObservable(IApi sparkApi, IDocumentFactory documentFactory, Configuration.ISettings settings, Configuration.IDevice device, Configuration.IVariable variable)
        {
            TimeSpan interval = variable.Interval ?? device.DefaultInterval ?? settings.SparkCore.DefaultInterval;
            string type = variable.Type ?? device.DefaultType ?? settings.SparkCore.DefaultType;

            string indexName = variable.Index ?? device.DefaultIndex ?? settings.SparkCore.DefaultIndex;
            Func<string> indexer = settings.ElasticSearch.GetIndexer(indexName);

            IObservable<IVariable> variableObservable = _sparkApi.ObserveVariable(device.Id, variable.Name, interval);

            IObservable<IVariable> handledObservable = variableObservable.Catch<IVariable, Exception>(exception => HandleAndResubscribe(variable, exception, variableObservable));
                    
            IObservable<IDocument> projectionObservable = handledObservable.Select(sparkVariable => documentFactory.CreateDocument(sparkVariable, indexer(), type));

            return projectionObservable;
        }

        public void Dispose()
        {
        }

        public IDisposable Subscribe(IObserver<IDocument> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
