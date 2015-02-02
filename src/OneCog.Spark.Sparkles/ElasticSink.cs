using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using OneCog.Spark.Sparkles.Document;
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
        private class IndexedDocument
        {
            public IDocument Document { get; set; }
            public string Index { get; set; }
        }

        private class IndexedDocumentResult : IndexedDocument
        {
            public Fallible<string> Result { get; set; }
        }

        private readonly IElasticClient _elasticClient;
        private readonly IClock _clock;

        private readonly IDictionary<string, Func<string>> _indexers;

        private Subject<IDocument> _subject;
        private IDisposable _subscription;

        public ElasticSink(Configuration.ISettings configuration, IElasticClient elasticClient, IClock clock)
        {
            _subject = new Subject<IDocument>();
            _elasticClient = elasticClient;
            _clock = clock;

            _indexers = BuildElasticSearchIndexerDictionary(configuration.ElasticSearch);
            _subscription = BuildElasticSearchWriteSubscription();
        }

        private Func<string> GetIndexer(Configuration.IIndex index)
        {
            if (!index.AppendDate)
            {
                return () => index.Name;
            }
            else
            {
                return () => string.Format("{0}-{1}", index.Name, _clock.UtcNow.ToString(index.DateFormat));
            }
        }

        private IDictionary<string, Func<string>> BuildElasticSearchIndexerDictionary(Configuration.IElasticSearch configuration)
        {
            return configuration.Indexes.Select(index => new { Name = index.Name, Indexer = GetIndexer(index) }).ToDictionary(value => value.Name, value => value.Indexer);
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
            IObservable<IndexedDocumentResult> indexSource = _subject
                .Do(document => Instrumentation.ElasticSearch.IndexingDocument(document))
                .Select(document => new IndexedDocument { Document = document, Index = _indexers[document.IndexName]() })
                .Select(indexedDocument => new IndexedDocumentResult { Document = indexedDocument.Document, Index = indexedDocument.Index, Result = _elasticClient.Index(indexedDocument.Index, indexedDocument.Document.Type, indexedDocument.Document.Body) });

            var handledSource = indexSource
                .Retry(exception => Instrumentation.ElasticSearch.IndexingException(exception))
                .Publish()
                .RefCount();

            IDisposable successHandler = handledSource.Where(indexing => indexing.Result.Success).Subscribe(indexing => Instrumentation.ElasticSearch.IndexedDocument(indexing.Document));
            IDisposable errorHandler = handledSource.Where(indexing => !indexing.Result.Success).Subscribe(indexing => Instrumentation.ElasticSearch.IndexingError(indexing.Document, indexing.Result.Error));

            return new CompositeDisposable(successHandler, errorHandler);
        }

        public void OnCompleted()
        {
            // Do nothing
        }

        public void OnError(Exception error)
        {
            Instrumentation.ElasticSearch.IndexingException(error);
        }

        public void OnNext(IDocument value)
        {
            _subject.OnNext(value);
        }
    }
}
