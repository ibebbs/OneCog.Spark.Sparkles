using Newtonsoft.Json;
using OneCog.Io.Spark;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Document
{
    public interface IFactory
    {
        IDocument CreateDocument(IVariable sparkVariable, string index, string type);
    }

    internal class Factory : IFactory
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        
        private IClock _clock;

        public Factory(IClock clock)
        {
            _clock = clock;
        }

        private string SerializeBody(object body)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (JsonTextWriter writer = new JsonTextWriter(stringWriter))
                {
                    writer.Formatting = Formatting.Indented;

                    Serializer.Serialize(writer, body);
                }

                return stringWriter.ToString();
            }
        }

        private object CreateBody(IVariable variable)
        {
            return new ForVariable
            {
                Name = variable.Name,
                Value = variable.Result,
                Timestamp = _clock.UtcNow
            };
        }

        public IDocument CreateDocument(IVariable sparkVariable, string index, string type)
        {
            object body = CreateBody(sparkVariable);

            return new Instance
            {
                IndexName = index,
                Type = type,
                Body = SerializeBody(body)
            };
        }
    }
}
