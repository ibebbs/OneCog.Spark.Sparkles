using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface ISettings
    {
        IElasticSearch ElasticSearch { get; }

        ISparkCore SparkCore { get; }
    }

    public class Settings : ISettings
    {

        IElasticSearch ISettings.ElasticSearch
        {
            get { return ElasticSearch; }
        }

        ISparkCore ISettings.SparkCore
        {
            get { return SparkCore; }
        }

        public ElasticSearch ElasticSearch { get; set; }

        public SparkCore SparkCore { get; set; }
    }
}
