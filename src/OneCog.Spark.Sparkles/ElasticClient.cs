using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public interface IElasticClient
    {
        Fallible<string> Index(string index, string type, string body);
    }

    internal class ElasticClient
    {
        private readonly IElasticsearchClient _elasticsearchClient;

        public ElasticClient(IElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
        }

        public Fallible<string> Index(string index, string type, string body)
        {
            var response = _elasticsearchClient.Index(index, type, body);

            return (response.Success) ? Fallible.Success(response.RequestUrl) : Fallible.Fail<string>(new InvalidOperationException(response.ServerError.Error));
        }
    }
}
