using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface IVariable
    {
        string Name { get; }

        TimeSpan? Interval { get; }

        string Index { get; }

        string Type { get; }

        bool OmitDuplicateReadings { get; }
    }

    public class Variable : IVariable
    {
        public string Name { get; set; }

        public TimeSpan? Interval { get; set; }

        public string Index { get; set; }

        public string Type { get; set; }

        public bool OmitDuplicateReadings { get; set; }
    }
}
