using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public interface IDevice
    {
        string Id { get; }

        TimeSpan? DefaultInterval { get; }

        string DefaultIndex { get; }

        string DefaultType { get; }

        IEnumerable<IVariable> Variables { get; }
    }

    public class Device : IDevice
    {
        IEnumerable<IVariable> IDevice.Variables
        {
            get { return Variables; }
        }

        public string Id { get; set; }

        public TimeSpan? DefaultInterval { get; set; }

        public string DefaultIndex { get; set; }

        public string DefaultType { get; set; }

        public IEnumerable<Variable> Variables { get; set; }
    }
}
