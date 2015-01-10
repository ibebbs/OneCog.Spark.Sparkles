using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public interface IElasticSink : IDisposable, IObserver<IDocument>
    {

    }

    internal class ElasticSink : IElasticSink
    {
        private Subject<IDocument> _subject;
        private IDisposable _subscription;
        private IElasticClient _elasticClient;

        public ElasticSink(IElasticClient elasticClient)
        {
            _subject = new Subject<IDocument>();
            _elasticClient = elasticClient;

            _subscription = BuildElasticSearchWriteSubscription();
        }

        public void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            if (_subject != null)
            {
                _subject.Dispose();
                _subject = null;
            }
        }

        private IDisposable BuildElasticSearchWriteSubscription()
        {
            var indexer = _subject
                .Do(document => Instrumentation.ElasticSearch.IndexingDocument(document))
                .Select(document => new { Document = document, Result = _elasticClient.Index(document.Index, document.Type, document.Body) })
                .Publish()
                .RefCount();

            IDisposable successHandler = indexer.Where(indexing => indexing.Result.Success).Subscribe(indexing => Instrumentation.ElasticSearch.IndexedDocument(indexing.Document));
            IDisposable errorHandler = indexer.Where(indexing => !indexing.Result.Success).Subscribe(indexing => Instrumentation.ElasticSearch.IndexingError(indexing.Document, indexing.Result.Error));

            return new CompositeDisposable(successHandler, errorHandler);
        }

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnNext(IDocument value)
        {
            _subject.OnNext(value);
        }
    }
}
