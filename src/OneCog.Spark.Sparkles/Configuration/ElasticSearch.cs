using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface IElasticSearch
    {
        Func<string> GetIndexer(string indexName);

        string Host { get; }

        string DefaultIndex { get; }

        IEnumerable<IIndex> Indexes { get; }

    }

    public class ElasticSearch : IElasticSearch
    {
        public Func<string> GetIndexer(string indexName)
        {
            IIndex index = Indexes.FirstOrDefault(i => indexName.Equals(i.Name, StringComparison.CurrentCultureIgnoreCase));

            if (index == null)
            {
                return () => indexName;
            }
            else if (!index.AppendDate)
            {
                return () => index.Name;
            }
            else
            {
                return () => string.Format("{0}-{1}", index.Name, DateTime.Now.ToString(index.DateFormat));
            }
        }

        IEnumerable<IIndex> IElasticSearch.Indexes
        {
            get { return Indexes ?? Enumerable.Empty<IIndex>(); }
        }

        string IElasticSearch.Host
        {
            get { return Host ?? "http://localhost:9200"; }
        }

        public string Host { get; set; }

        public string DefaultIndex { get; set; }

        public IEnumerable<Index> Indexes { get; set; }
    }
}
