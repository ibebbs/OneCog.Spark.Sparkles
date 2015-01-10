using System;
using System.Collections.Generic;
using System.Linq;
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

        private IDisposable _subscription;

        public Service(ISparkSource source, IElasticSink sink)
        {
            _source = source;
            _sink = sink;
        }

        public void Start()
        {
            _subscription = _source.Subscribe(_sink);
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
