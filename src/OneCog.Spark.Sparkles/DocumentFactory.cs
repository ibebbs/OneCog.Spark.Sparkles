using OneCog.Io.Spark;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public interface IDocumentFactory
    {
        IDocument CreateDocument(IVariable sparkVariable, string index, string type);
    }

    internal class DocumentFactory : IDocumentFactory
    {
        public IDocument CreateDocument(IVariable sparkVariable, string index, string type)
        {
            return new Document
            {
                Index = index,
                Type = type,
                Body = Variable.ToJsonString(sparkVariable)
            };
        }
    }
}
