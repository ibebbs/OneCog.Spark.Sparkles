using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface IIndex
    {
        string Name { get; }

        bool AppendDate { get; }

        string DateFormat { get; }
    }

    public class Index : IIndex
    {
        public string Name { get; set; }

        public bool AppendDate { get; set; }

        public string DateFormat { get; set; }
    }
}
