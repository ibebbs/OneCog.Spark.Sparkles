using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public interface IMonitor : IObservable<EventEntry>
    {

    }

    internal class Monitor : IMonitor
    {
        private readonly ObservableEventListener _sparkListener;
        private readonly ObservableEventListener _elasticSearchListener;
        private readonly IObservable<EventEntry> _observable;

        public Monitor()
        {
            _sparkListener = new ObservableEventListener();
            _sparkListener.EnableEvents((EventSource)Instrumentation.SparkCore, EventLevel.Warning);

            _elasticSearchListener = new ObservableEventListener();
            _elasticSearchListener.EnableEvents((EventSource)Instrumentation.ElasticSearch, EventLevel.Warning);

            _observable = Observable.Merge(_sparkListener, _elasticSearchListener).Publish().RefCount();
        }

        public IDisposable Subscribe(IObserver<EventEntry> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
