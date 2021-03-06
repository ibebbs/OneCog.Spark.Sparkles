﻿using Elasticsearch.Net;
using OneCog.Io.Spark;
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

    internal class ElasticClient : IElasticClient
    {
        private readonly IElasticsearchClient _elasticsearchClient;

        public ElasticClient(IElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
        }

        public Fallible<string> Index(string index, string type, string body)
        {
            try
            {
                var response = _elasticsearchClient.Index(index, type, body);

                return (response.Success) ? Fallible.FromValue(response.RequestUrl) : Fallible.FromException<string>(new InvalidOperationException(response.ServerError.Error));
            }
            catch (Exception error)
            {
                return Fallible.FromException<string>(error);
            }
        }
    }
}
