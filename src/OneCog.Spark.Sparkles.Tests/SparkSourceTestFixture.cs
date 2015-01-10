using FakeItEasy;
using NUnit.Framework;
using OneCog.Io.Spark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles.Tests
{
    [TestFixture]
    public class SparkSourceTestFixture
    {
        private Configuration.ISettings _settings;
        private Io.Spark.IApi _sparkApi;
        private IDocumentFactory _documentFactory;
        private SparkSource _subject;
        private Configuration.ISparkCore _sparkCoreSettings;
        private static Configuration.Device _deviceA;
        private static Configuration.Device _deviceB;
        private Configuration.IElasticSearch _elasticSearchSettings;
        private static Configuration.Index _tempIndex;
        private static Configuration.Index _lightIndex;
        private static Configuration.Index _humidityIndex;

        [TestFixtureSetUp]
        public static void TestSetup()
        {
            _deviceA = new Configuration.Device
            {
                Id = "DeviceA",
                DefaultIndex = "DeviceAIndex",
                DefaultInterval = TimeSpan.FromSeconds(75),
                DefaultType = "DeviceAType",
                Variables = new[] {
                    new Configuration.Variable { Name = "temp", Index = "tempIndex", Interval = TimeSpan.FromSeconds(10), Type = "tempType", OmitDuplicateReadings = true },
                    new Configuration.Variable { Name = "humidity", Index = "humidityIndex", Interval = TimeSpan.FromSeconds(20), Type = "humidityType", OmitDuplicateReadings = true }
                }
            };

            _deviceB = new Configuration.Device
            {
                Id = "DeviceB",
                DefaultIndex = "DeviceBIndex",
                DefaultInterval = TimeSpan.FromSeconds(75),
                DefaultType = "DeviceBType",
                Variables = new[] {
                    new Configuration.Variable { Name = "light", Index = "lightIndex", Interval = TimeSpan.FromSeconds(30), Type = "lightType", OmitDuplicateReadings = true }
                }
            };

            _tempIndex = new Configuration.Index { Name = "tempIndex", AppendDate = true, DateFormat= "yyy-MM-dd" };
            _lightIndex = new Configuration.Index { Name = "lightIndex", AppendDate = true };
            _humidityIndex = new Configuration.Index { Name = "humidityIndex", AppendDate = false };
        }

        [SetUp]
        public void Setup()
        {
            _sparkCoreSettings = A.Fake<Configuration.ISparkCore>();
            A.CallTo(() => _sparkCoreSettings.AccessToken).Returns("0123456789abcdef123456");
            A.CallTo(() => _sparkCoreSettings.DefaultIndex).Returns("CoreIndex");
            A.CallTo(() => _sparkCoreSettings.DefaultInterval).Returns(TimeSpan.FromSeconds(100));
            A.CallTo(() => _sparkCoreSettings.DefaultType).Returns("CoreType");
            A.CallTo(() => _sparkCoreSettings.Devices).Returns(new[] { _deviceA, _deviceB });

            _elasticSearchSettings = A.Fake<Configuration.IElasticSearch>();
            A.CallTo(() => _elasticSearchSettings.Host).Returns("http://localhost:9220");
            A.CallTo(() => _elasticSearchSettings.Indexes).Returns(new[] { _tempIndex, _humidityIndex, _lightIndex });

            _settings = new Configuration.Settings
            {
                SparkCore = _sparkCoreSettings,
                ElasticSearch = _elasticSearchSettings
            };

            _sparkApi = A.Fake<Io.Spark.IApi>();
            _documentFactory = A.Fake<IDocumentFactory>();

            _subject = new SparkSource(_settings, _sparkApi, _documentFactory);
        }

        [Test]
        public void ShouldSubscribeToAllVariablesForAllDevicesWhenSubscribed()
        {
            IObservable<IVariable> tempObservable = A.Fake<IObservable<IVariable>>();
            IObservable<IVariable> lightObservable = A.Fake<IObservable<IVariable>>();
            IObservable<IVariable> humidityObservable = A.Fake<IObservable<IVariable>>();

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(tempObservable);
            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "humidity", TimeSpan.FromSeconds(20), null)).Returns(humidityObservable);
            A.CallTo(() => _sparkApi.ObserveVariable("DeviceB", "light", TimeSpan.FromSeconds(30), null)).Returns(lightObservable);

            _subject.Subscribe();

            A.CallTo(() => tempObservable.Subscribe(A<IObserver<IVariable>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => lightObservable.Subscribe(A<IObserver<IVariable>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => humidityObservable.Subscribe(A<IObserver<IVariable>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldUseDocumentFactoryToBuildDocumentWhenVariableEmittedBySubscription()
        {
            IVariable variable = A.Fake<IVariable>();
            Subject<IVariable> tempObservable = new Subject<IVariable>();

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(tempObservable);

            _subject.Subscribe();

            tempObservable.OnNext(variable);

            A.CallTo(() => _documentFactory.CreateDocument(variable, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldEmitDocumentWhenVariableEmittedBySubscription()
        {
            IVariable variable = A.Fake<IVariable>();
            List<IDocument> documents = new List<IDocument>();

            Subject<IVariable> tempObservable = new Subject<IVariable>();

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(tempObservable);

            _subject.Subscribe(documents.Add);

            tempObservable.OnNext(variable);

            Assert.That(documents.Count, Is.EqualTo(1));
        }
    }
}
