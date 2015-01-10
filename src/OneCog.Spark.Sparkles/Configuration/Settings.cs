using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface ISettings
    {
        IElasticSearch ElasticSearch { get; set; }

        ISparkCore SparkCore { get; set; }

        IIndex GetIndexForVariable(IVariable variable);
    }

    public class Settings : ISettings
    {
        public IIndex GetIndexForVariable(IVariable variable)
        {
            throw new NotImplementedException();
        }

        public IElasticSearch ElasticSearch { get; set; }

        public ISparkCore SparkCore { get; set; }
    }
}
