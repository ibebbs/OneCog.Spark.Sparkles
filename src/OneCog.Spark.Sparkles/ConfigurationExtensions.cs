using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public static class ConfigurationExtensions
    {
        public static bool TryGetIndex(this Configuration.IElasticSearch elasticSearch, string indexName, out Configuration.IIndex index)
        {
            index = elasticSearch.Indexes.Where(i => i.Name == indexName).FirstOrDefault();

            return index != null;
        }
    }
}
