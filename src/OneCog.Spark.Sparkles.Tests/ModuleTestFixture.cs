using FakeItEasy;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Tests
{
    [TestFixture]
    public class ModuleTestFixture
    {
        [Test]
        public void ShouldBeAbleToResolveService()
        {
            Configuration.IProvider configurationProvider = A.Fake<Configuration.IProvider>();

            A.CallTo(() => configurationProvider.GetSettings()).Returns(
                new Configuration.Settings
                {
                    ElasticSearch = new Configuration.ElasticSearch(),
                    SparkCore = new Configuration.SparkCore()
                }
            );

            IKernel kernel = new StandardKernel(new Module(), new Io.Spark.Ninject.Module("TEST"));
            kernel.Rebind<Configuration.IProvider>().ToConstant(configurationProvider);

            IService service = kernel.Get<IService>();

            Assert.That(service, Is.Not.Null);
        }
    }
}
