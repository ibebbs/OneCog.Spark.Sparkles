using FakeItEasy;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using OneCog.Io.Spark;
using OneCog.Spark.Sparkles.Document;
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
        private static Configuration.Device _deviceA;
        private static Configuration.Device _deviceB;
        private static Configuration.Index _tempIndex;
        private static Configuration.Index _lightIndex;
        private static Configuration.Index _humidityIndex;

        private Configuration.ISettings _settings;
        private Configuration.IElasticSearch _elasticSearchSettings;
        private Configuration.ISparkCore _sparkCoreSettings;
        private Io.Spark.IApi _sparkApi;
        private Document.IFactory _documentFactory;
        private ISchedulerProvider _schedulerProvider;
        private TestScheduler _testScheduler;
        private SparkSource _subject;

        [TestFixtureSetUp]
        public static void TestSetup()
        {
            _deviceA = new Configuration.Device
            {
                Id = "DeviceA",
                DefaultIndexName = "DeviceAIndex",
                DefaultInterval = TimeSpan.FromSeconds(75),
                DefaultType = "DeviceAType",
                Variables = new[] {
                    new Configuration.Variable { Name = "temp", IndexName = "tempIndex", Interval = TimeSpan.FromSeconds(10), Type = "tempType", OmitDuplicateReadings = true },
                    new Configuration.Variable { Name = "humidity", IndexName = "humidityIndex", Interval = TimeSpan.FromSeconds(20), Type = "humidityType", OmitDuplicateReadings = true }
                }
            };

            _deviceB = new Configuration.Device
            {
                Id = "DeviceB",
                DefaultIndexName = "DeviceBIndex",
                DefaultInterval = TimeSpan.FromSeconds(75),
                DefaultType = "DeviceBType",
                Variables = new[] {
                    new Configuration.Variable { Name = "light", IndexName = "lightIndex", Interval = TimeSpan.FromSeconds(30), Type = "lightType", OmitDuplicateReadings = true }
                }
            };

            _tempIndex = new Configuration.Index { Name = "tempIndex", AppendDate = true, DateFormat = "yyy-MM-dd" };
            _lightIndex = new Configuration.Index { Name = "lightIndex", AppendDate = true };
            _humidityIndex = new Configuration.Index { Name = "humidityIndex", AppendDate = false };
        }

        [SetUp]
        public void Setup()
        {
            _sparkCoreSettings = A.Fake<Configuration.ISparkCore>();
            A.CallTo(() => _sparkCoreSettings.AccessToken).Returns("0123456789abcdef123456");
            A.CallTo(() => _sparkCoreSettings.DefaultIndexName).Returns("CoreIndex");
            A.CallTo(() => _sparkCoreSettings.DefaultInterval).Returns(TimeSpan.FromSeconds(100));
            A.CallTo(() => _sparkCoreSettings.DefaultType).Returns("CoreType");
            A.CallTo(() => _sparkCoreSettings.Devices).Returns(new[] { _deviceA, _deviceB });

            _elasticSearchSettings = A.Fake<Configuration.IElasticSearch>();
            A.CallTo(() => _elasticSearchSettings.Host).Returns("http://localhost:9220");
            A.CallTo(() => _elasticSearchSettings.Indexes).Returns(new[] { _tempIndex, _humidityIndex, _lightIndex });

            _settings = A.Fake<Configuration.ISettings>();
            A.CallTo(() => _settings.SparkCore).Returns(_sparkCoreSettings);
            A.CallTo(() => _settings.ElasticSearch).Returns(_elasticSearchSettings);

            _sparkApi = A.Fake<Io.Spark.IApi>();
            _documentFactory = A.Fake<Document.IFactory>();
            _schedulerProvider = A.Fake<ISchedulerProvider>();
            _testScheduler = new TestScheduler();
            A.CallTo(() => _schedulerProvider.AsyncScheduler).Returns(_testScheduler);

            _subject = new SparkSource(_settings, _sparkApi, _documentFactory, _schedulerProvider);
        }

        [Test]
        public void ShouldSubscribeToAllVariablesForAllDevicesWhenSubscribed()
        {
            IObservable<Fallible<IVariable>> tempObservable = A.Fake<IObservable<Fallible<IVariable>>>();
            IObservable<Fallible<IVariable>> lightObservable = A.Fake<IObservable<Fallible<IVariable>>>();
            IObservable<Fallible<IVariable>> humidityObservable = A.Fake<IObservable<Fallible<IVariable>>>();

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(tempObservable);
            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "humidity", TimeSpan.FromSeconds(20), null)).Returns(humidityObservable);
            A.CallTo(() => _sparkApi.ObserveVariable("DeviceB", "light", TimeSpan.FromSeconds(30), null)).Returns(lightObservable);

            _subject.Subscribe();

            A.CallTo(() => tempObservable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => lightObservable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => humidityObservable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldUseDocumentFactoryToBuildDocumentWhenVariableEmittedBySubscription()
        {
            IVariable variable = A.Fake<IVariable>();
            Fallible<IVariable> fallible = Fallible.FromValue(variable);
            Subject<Fallible<IVariable>> tempObservable = new Subject<Fallible<IVariable>>();

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(tempObservable);

            _subject.Subscribe();

            tempObservable.OnNext(fallible);

            A.CallTo(() => _documentFactory.CreateDocument(variable, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldEmitDocumentWhenVariableEmittedBySubscription()
        {
            IVariable variable = A.Fake<IVariable>();
            Fallible<IVariable> fallible = Fallible.FromValue(variable);

            List<IDocument> documents = new List<IDocument>();

            Subject<Fallible<IVariable>> tempObservable = new Subject<Fallible<IVariable>>();

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(tempObservable);

            _subject.Subscribe(documents.Add);

            tempObservable.OnNext(fallible);

            Assert.That(documents.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShouldResubscribeAfterAnErrorObservingVariable()
        {
            IVariable variable = A.Fake<IVariable>();
            Fallible<IVariable> fallible = Fallible.FromValue(variable);

            List<IDocument> documents = new List<IDocument>();

            IObserver<Fallible<IVariable>> observer = null;
            IObservable<Fallible<IVariable>> observable = A.Fake<IObservable<Fallible<IVariable>>>();

            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).Invokes(call => observer = call.GetArgument<IObserver<Fallible<IVariable>>>(0));

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(observable);

            _subject.Subscribe(documents.Add);

            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.That(observer, Is.Not.Null);

            observer.OnNext(fallible);

            Assert.That(documents.Count, Is.EqualTo(1));

            observer.OnError(new TimeoutException());

            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);

            Assert.That(observer, Is.Not.Null);

            observer.OnNext(fallible);

            Assert.That(documents.Count, Is.EqualTo(2));
        }

        [Test]
        public void ShouldResubscribeAfterNotReceivingAnValueAfterFiveTimesTheInterval()
        {
            IVariable variable = A.Fake<IVariable>();
            Fallible<IVariable> fallible = Fallible.FromValue(variable);

            List<IDocument> documents = new List<IDocument>();

            IObserver<Fallible<IVariable>> observer = null;
            IObservable<Fallible<IVariable>> observable = A.Fake<IObservable<Fallible<IVariable>>>();

            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).Invokes(call => observer = call.GetArgument<IObserver<Fallible<IVariable>>>(0));

            A.CallTo(() => _sparkApi.ObserveVariable("DeviceA", "temp", TimeSpan.FromSeconds(10), null)).Returns(observable);

            _subject.Subscribe(documents.Add);

            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.That(observer, Is.Not.Null);

            observer.OnNext(fallible);

            Assert.That(documents.Count, Is.EqualTo(1));

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            A.CallTo(() => observable.Subscribe(A<IObserver<Fallible<IVariable>>>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);

            Assert.That(observer, Is.Not.Null);

            observer.OnNext(fallible);

            Assert.That(documents.Count, Is.EqualTo(2));
        }
    }
}
