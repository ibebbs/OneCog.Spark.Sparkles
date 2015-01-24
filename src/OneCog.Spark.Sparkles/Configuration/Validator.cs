using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Configuration
{
    public static class Validator
    {
        private static readonly IEnumerable<Tuple<Func<IVariable, ISettings, bool>, Action<IVariable>>> VariableValidators = new Tuple<Func<IVariable, ISettings, bool>, Action<IVariable>>[]
        {
            Tuple.Create<Func<IVariable, ISettings, bool>, Action<IVariable>>((variable, settings) => !variable.CanResolveIndexName(settings), variable => Instrumentation.Configuration.SparkCoreVariableUnknownIndexName(variable))

            /*More validators*/
        };

        private static readonly IEnumerable<Tuple<Func<IDevice, ISettings, bool>, Action<IDevice>>> DeviceValidators = new Tuple<Func<IDevice, ISettings, bool>, Action<IDevice>>[]
        {
            Tuple.Create<Func<IDevice, ISettings, bool>, Action<IDevice>>((device, settings) => !device.Variables.Any(), device => Instrumentation.Configuration.SparkCoreDeviceHasNoVariables(device)),
            Tuple.Create<Func<IDevice, ISettings, bool>, Action<IDevice>>((device, settings) => !device.Variables.Aggregate(false, (state, variable) => variable.Validate(settings)), device => Instrumentation.Configuration.SparkCoreDeviceVariablesInvalid(device))
        };

        private static readonly IEnumerable<Tuple<Func<ISparkCore, ISettings, bool>, Action<ISparkCore>>> SparkCoreValidators = new Tuple<Func<ISparkCore, ISettings, bool>, Action<ISparkCore>>[]
        {
            Tuple.Create<Func<ISparkCore, ISettings, bool>, Action<ISparkCore>>((sparkCore, settings) => string.IsNullOrWhiteSpace(sparkCore.AccessToken), settings => Instrumentation.Configuration.SparkCoreAccessTokenNotSet()),
            Tuple.Create<Func<ISparkCore, ISettings, bool>, Action<ISparkCore>>((sparkCore, settings) => string.Equals(sparkCore.AccessToken, Default.AccessToken), settings => Instrumentation.Configuration.SparkCoreAccessTokenNotSet()),
            Tuple.Create<Func<ISparkCore, ISettings, bool>, Action<ISparkCore>>((sparkCore, settings) => !sparkCore.Devices.Any(), settings => Instrumentation.Configuration.NoSparkCoreDevices()),
            Tuple.Create<Func<ISparkCore, ISettings, bool>, Action<ISparkCore>>((sparkCore, settings) => !sparkCore.Devices.Aggregate(false, (state, device) => device.Validate(settings) || state), settings => {})
        };

        private static readonly IEnumerable<Tuple<Func<IElasticSearch, ISettings, bool>, Action<IElasticSearch>>> ElasticSearchValidators = new Tuple<Func<IElasticSearch, ISettings, bool>, Action<IElasticSearch>>[]
        {
            Tuple.Create<Func<IElasticSearch, ISettings, bool>, Action<IElasticSearch>>((elasticSearch, settings) => string.IsNullOrWhiteSpace(elasticSearch.Host), settings => Instrumentation.Configuration.ElasticSearchHostNotSet())
        };

        public static bool CanResolveIndexName(this IVariable variable, ISettings settings)
        {
            IDevice device = settings.SparkCore.Devices.First(d => d.Variables.Contains(variable));

            string indexName = variable.IndexName ?? device.DefaultIndexName ?? settings.SparkCore.DefaultIndexName;

            return (!string.IsNullOrWhiteSpace(indexName) && settings.ElasticSearch.Indexes.Any(index => string.Equals(index.Name, indexName)));
        }

        public static bool Validate(this IVariable variable, ISettings settings)
        {
            var failed = VariableValidators.Where(validator => validator.Item1(variable, settings)).Do(validator => validator.Item2(variable)).ToArray();

            return !failed.Any();
        }

        public static bool Validate(this IDevice device, ISettings settings)
        {
            var failed = DeviceValidators.Where(validator => validator.Item1(device, settings)).Do(validator => validator.Item2(device)).ToArray();

            return !failed.Any();
        }

        public static bool Validate(this ISparkCore sparkCore, ISettings settings)
        {
            var failed = SparkCoreValidators.Where(validator => validator.Item1(sparkCore, settings)).Do(validator => validator.Item2(sparkCore)).ToArray();

            return !failed.Any();
        }

        public static bool Validate(this IElasticSearch elasticSearch, ISettings settings)
        {
            var failed = ElasticSearchValidators.Where(validator => validator.Item1(elasticSearch, settings)).Do(validator => validator.Item2(elasticSearch)).ToArray();

            return !failed.Any();
        }

        public static bool Validate(this ISettings settings)
        {
            using (ObservableEventListener configurationWarnings = new ObservableEventListener())
            {
                configurationWarnings.EnableEvents((EventSource)Instrumentation.Configuration, EventLevel.LogAlways);
                configurationWarnings.LogToConsole();

                try
                {
                    return settings.SparkCore.Validate(settings) && settings.ElasticSearch.Validate(settings);
                }
                finally
                {
                    configurationWarnings.DisableEvents((EventSource)Instrumentation.Configuration);
                }
            }
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }

            return source;
        }
    }
}
