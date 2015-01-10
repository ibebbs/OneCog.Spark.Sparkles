using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using FakeItEasy;
using NUnit.Framework;
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
        [Test]
        public void ShouldIndexDocument()
        {
            IElasticClient elasticClient = A.Fake<IElasticClient>();

            IDocument document = A.Fake<IDocument>();
            A.CallTo(() => document.Index).Returns("documentIndex");
            A.CallTo(() => document.Type).Returns("documentType");
            A.CallTo(() => document.Body).Returns("documentBody");

            A.CallTo(() => elasticClient.Index("documentIndex", "documentType", "documentBody")).Returns(Fallible.Success("Test"));

            ElasticSink subject = new ElasticSink(elasticClient);

            subject.OnNext(document);

            A.CallTo(() => elasticClient.Index("documentIndex", "documentType", "documentBody")).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
