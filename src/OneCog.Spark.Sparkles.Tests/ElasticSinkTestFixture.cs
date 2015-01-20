using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using FakeItEasy;
using NUnit.Framework;
using OneCog.Spark.Sparkles.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Tests
{
    [TestFixture]
    public class ElasticSinkTestFixture
    {
        private static Configuration.Index _tempIndex;
        private static Configuration.Index _lightIndex;
        private static Configuration.Index _humidityIndex;

        private Configuration.IElasticSearch _elasticSearchSettings;
        private Configuration.ISettings _settings;
        private IClock _clock;

        [TestFixtureSetUp]
        public static void TestSetup()
        {
            _tempIndex = new Configuration.Index { Name = "tempIndex", AppendDate = true, DateFormat = "yyy.MM.dd" };
            _lightIndex = new Configuration.Index { Name = "lightIndex", AppendDate = true };
            _humidityIndex = new Configuration.Index { Name = "humidityIndex", AppendDate = false };
        }

        [SetUp]
        public void Setup()
        {
            _elasticSearchSettings = A.Fake<Configuration.IElasticSearch>();
            A.CallTo(() => _elasticSearchSettings.Host).Returns("http://localhost:9220");
            A.CallTo(() => _elasticSearchSettings.Indexes).Returns(new[] { _tempIndex, _humidityIndex, _lightIndex });

            _settings = A.Fake<Configuration.ISettings>();
            A.CallTo(() => _settings.SparkCore).Returns(A.Fake<Configuration.ISparkCore>());
            A.CallTo(() => _settings.ElasticSearch).Returns(_elasticSearchSettings);

            _clock = A.Fake<IClock>();
            A.CallTo(() => _clock.UtcNow).Returns(DateTime.UtcNow);
        }

        [Test]
        public void ShouldIndexDocument()
        {
            IElasticClient elasticClient = A.Fake<IElasticClient>();

            IDocument document = A.Fake<IDocument>();
            A.CallTo(() => document.IndexName).Returns("humidityIndex");
            A.CallTo(() => document.Type).Returns("documentType");
            A.CallTo(() => document.Body).Returns("documentBody");

            A.CallTo(() => elasticClient.Index("humidityIndex", "documentType", "documentBody")).Returns(Fallible.Success("Test"));

            ElasticSink subject = new ElasticSink(_settings, elasticClient, _clock);

            subject.OnNext(document);

            A.CallTo(() => elasticClient.Index("humidityIndex", "documentType", "documentBody")).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldUseIndexersToDetermineIndex()
        {
            IElasticClient elasticClient = A.Fake<IElasticClient>();

            DateTime indexDate = DateTime.UtcNow;
            A.CallTo(() => _clock.UtcNow).Returns(indexDate);

            string index = string.Format("{0}-{1}", "tempIndex", indexDate.ToString("yyyy.MM.dd"));

            IDocument document = A.Fake<IDocument>();
            A.CallTo(() => document.IndexName).Returns("tempIndex");
            A.CallTo(() => document.Type).Returns("documentType");
            A.CallTo(() => document.Body).Returns("documentBody");

            A.CallTo(() => elasticClient.Index(index, "documentType", "documentBody")).Returns(Fallible.Success("Test"));

            ElasticSink subject = new ElasticSink(_settings, elasticClient, _clock);

            subject.OnNext(document);

            A.CallTo(() => elasticClient.Index(index, "documentType", "documentBody")).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
