using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public interface IService
    {
        void Start();
        void Stop();
    }

    internal class Service : IService
    {
        private readonly ISparkSource _source;
        private readonly IElasticSink _sink;
        private readonly IMonitor _monitor;

        private IDisposable _subscription;

        public Service(ISparkSource source, IElasticSink sink, IMonitor monitor)
        {
            _source = source;
            _sink = sink;
            _monitor = monitor;
        }

        public void Start()
        {
            _subscription = new CompositeDisposable(
                _monitor.LogToConsole(),
                _source.Subscribe(_sink)
            );
        }

        public void Stop()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }
    }
}
