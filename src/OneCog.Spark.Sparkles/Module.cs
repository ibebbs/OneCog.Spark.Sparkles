﻿using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using EventSourceProxy;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using OneCog.Io.Spark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public class Module : NinjectModule
    {
        private IElasticsearchClient BuildElasticSearchClient(IContext context)
        {
            Configuration.ISettings settings = context.Kernel.Get<Configuration.ISettings>();

            ConnectionConfiguration config = new ConnectionConfiguration(new Uri(settings.ElasticSearch.Host));
            ElasticsearchClient client = new ElasticsearchClient(config);

            return client;
        }

        private Configuration.ISettings GetConfigurationSettings(IContext context)
        {
            Configuration.IProvider provider = context.Kernel.Get<Configuration.IProvider>();

            return provider.GetSettings();
        }

        public override void Load()
        {
            Bind<IClock>().To<Clock>().InSingletonScope();
            Bind<ISchedulerProvider>().To<SchedulerProvider>().InSingletonScope();

            Bind<Configuration.IProvider>().To<Configuration.Provider>().InSingletonScope();
            Bind<Configuration.ISettings>().ToMethod(GetConfigurationSettings).InSingletonScope();

            Bind<Document.IFactory>().To<Document.Factory>().InSingletonScope();

            Bind<IElasticsearchClient>().ToMethod(BuildElasticSearchClient);
            Bind<IElasticClient>().To<ElasticClient>();
            Bind<IElasticSink>().To<ElasticSink>();

            Bind<ISparkSource>().To<SparkSource>();

            Bind<IMonitor>().To<Monitor>().InSingletonScope();

            Bind<Service>().ToSelf();
            Bind<IService>().ToMethod(context => EventSourceProxy.TracingProxy.Create<IService>(context.Kernel.Get<Service>()));
        }
    }
}
