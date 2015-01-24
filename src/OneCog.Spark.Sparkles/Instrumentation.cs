using EventSourceProxy;
using OneCog.Spark.Sparkles.Configuration;
using OneCog.Spark.Sparkles.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    [EventSourceImplementation(Name = "OneCog-Spark-Sparkles-SparkCore")]
    public interface ISparkCore
    {
        [Event(1, Message = "StartObservingVariable", Level = EventLevel.Informational)]
        void StartObservingVariable(IVariable variable);

        [Event(2, Message = "StopObservingVariable", Level = EventLevel.Informational)]
        void StopObservingVariable(IVariable variable);

        [Event(3, Message = "ErrorWhileObservingVariable", Level = EventLevel.Error)]
        void ErrorWhileObservingVariable(IVariable variable, Exception exception);
    }

    [EventSourceImplementation(Name = "OneCog-Spark-Sparkles-ElasticSearch")]
    public interface IElasticSearch
    {
        [Event(1, Message = "IndexingDocument", Level = EventLevel.Informational)]
        void IndexingDocument(IDocument document);

        [Event(2, Message = "IndexedDocument", Level = EventLevel.Informational)]
        void IndexedDocument(IDocument document);

        [Event(3, Message = "IndexingError", Level = EventLevel.Warning)]
        void IndexingError(IDocument document, Exception exception);

        [Event(4, Message = "IndexingException", Level = EventLevel.Error)]
        void IndexingException(Exception exception);
    }

    [EventSourceImplementation(Name = "OneCog-Spark-Sparkles-Configuration")]
    public interface IConfiguration
    {
        void ElasticSearchHostNotSet();

        void SparkCoreAccessTokenNotSet();

        void NoSparkCoreDevices();

        void SparkCoreDeviceHasNoVariables(IDevice device);

        void SparkCoreVariableUnknownIndexName(IDevice variable);

        void SparkCoreVariableUnknownIndexName(IVariable variable);

        void SparkCoreDeviceVariablesInvalid(IDevice device);
    }

    public static class Instrumentation
    {
        private static readonly Lazy<ISparkCore> _sparkCore = new Lazy<ISparkCore>(() => EventSourceImplementer.GetEventSourceAs<ISparkCore>());

        private static readonly Lazy<IElasticSearch> _elasticSearch = new Lazy<IElasticSearch>(() => EventSourceImplementer.GetEventSourceAs<IElasticSearch>());

        private static readonly Lazy<IConfiguration> _configuration = new Lazy<IConfiguration>(() => EventSourceImplementer.GetEventSourceAs<IConfiguration>());

        public static ISparkCore SparkCore
        {
            get { return _sparkCore.Value; }
        }

        public static IElasticSearch ElasticSearch
        {
            get { return _elasticSearch.Value; }
        }

        public static IConfiguration Configuration
        {
            get { return _configuration.Value; }
        }
    }
}
