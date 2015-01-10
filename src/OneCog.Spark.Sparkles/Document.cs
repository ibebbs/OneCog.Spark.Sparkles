using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public interface IDocument
    {
        string Index { get; }

        string Type { get; }

        string Body { get; }
    }

    internal class Document : IDocument
    {
        public string Index { get; set; }

        public string Type { get; set; }

        public string Body { get; set; }
    }
}
