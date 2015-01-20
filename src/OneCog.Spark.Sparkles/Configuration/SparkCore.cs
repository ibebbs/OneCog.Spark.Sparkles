using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface ISparkCore
    {
        string AccessToken { get; }

        TimeSpan DefaultInterval { get; }

        string DefaultIndexName { get; }

        string DefaultType { get; }

        IEnumerable<IDevice> Devices { get; }
    }

    public class SparkCore : ISparkCore
    {
        IEnumerable<IDevice> ISparkCore.Devices
        {
            get { return Devices ?? Enumerable.Empty<IDevice>(); }
        }

        public string AccessToken { get; set; }

        public TimeSpan DefaultInterval { get; set; }

        public string DefaultIndexName { get; set; }

        public string DefaultType { get; set; }

        public IEnumerable<Device> Devices { get; set; }
    }
}
