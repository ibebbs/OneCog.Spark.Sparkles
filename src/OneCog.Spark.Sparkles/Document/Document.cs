using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Document
{
    public interface IDocument
    {
        string IndexName { get; }

        string Type { get; }

        string Body { get; }
    }

    internal class Instance : IDocument
    {
        public string IndexName { get; set; }

        public string Type { get; set; }

        public string Body { get; set; }
    }
}
