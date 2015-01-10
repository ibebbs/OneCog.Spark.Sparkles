using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface ISparkCore
    {
        string AccessToken { get; set; }

        TimeSpan DefaultInterval { get; set; }

        string DefaultIndex { get; set; }

        string DefaultType { get; set; }

        IEnumerable<IDevice> Devices { get; set; }
    }

    public class SparkCore : ISparkCore
    {
        public string AccessToken { get; set; }

        public TimeSpan DefaultInterval { get; set; }

        public string DefaultIndex { get; set; }

        public string DefaultType { get; set; }

        public IEnumerable<IDevice> Devices { get; set; }
    }
}
