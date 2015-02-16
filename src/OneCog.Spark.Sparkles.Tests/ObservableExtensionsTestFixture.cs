using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using Microsoft.Reactive.Testing;

namespace OneCog.Spark.Sparkles.Tests
{
    [TestFixture]
    public class ObservableExtensionsTestFixture
    {
        public class RetryShould
        {
            [Test]
            public void SubscribeToTheSourceObservable()
            {
                IObservable<int> observabe = A.Fake<IObservable<int>>();

                observabe.Retry(A.Fake<IObserver<Exception>>()).Subscribe();

                A.CallTo(() => observabe.Subscribe(A<IObserver<int>>.Ignored)).MustHaveHappened();
            }

            [Test]
            public void CallOnErrorWhenAnExceptionIsEncountered()
            {
                TestScheduler scheduler = new TestScheduler();
                IObservable<int> observable = scheduler.CreateColdObservable<int>(new [] {
                    new Recorded<Notification<int>>(10, Notification.CreateOnError<int>(new InvalidOperationException()))
                });

                IObserver<Exception> errors = A.Fake<IObserver<Exception>>();

                observable.Retry(errors).Subscribe();

                scheduler.AdvanceBy(10);
                
                A.CallTo(() => errors.OnNext(A<Exception>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            }

            [Test]
            public void ReceiveItemsAfterAnExceptionIsEncountered()
            {
                TestScheduler scheduler = new TestScheduler();
                IObservable<int> observableA = scheduler.CreateColdObservable<int>(new[] {
                    new Recorded<Notification<int>>(10, Notification.CreateOnError<int>(new InvalidOperationException()))
                });

                IObservable<int> observableB = scheduler.CreateColdObservable<int>(new[] {
                    new Recorded<Notification<int>>(10, Notification.CreateOnNext<int>(314))
                });

                Queue<IObservable<int>> observables = new Queue<IObservable<int>>(new [] { observableA, observableB });

                IObservable<int> observable = A.Fake<IObservable<int>>();
                A.CallTo(() => observable.Subscribe(A<IObserver<int>>.Ignored))
                 .Invokes(call => observables.Dequeue().Subscribe(call.GetArgument<IObserver<int>>(0)));

                IObserver<Exception> errors = A.Fake<IObserver<Exception>>();
                IObserver<int> values = A.Fake<IObserver<int>>();

                observable.Retry(errors).Subscribe(values);

                scheduler.AdvanceBy(20);

                A.CallTo(() => values.OnNext(314)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        public class ZipLatestShould
        {
            [Test]
            public void NotEmitUnitBothSourcesHaveProducedAValue()
            {
                Subject<int> source1 = new Subject<int>();
                Subject<int> source2 = new Subject<int>();

                List<Tuple<int,int>> actual = new List<Tuple<int,int>>();
                Tuple<int,int> expected = Tuple.Create(3, 1);

                using (IDisposable subscription = ObservableExtensions.ZipLatest(source1, source2, Tuple.Create).Subscribe(actual.Add))
                {
                    source1.OnNext(1);
                    source1.OnNext(2);
                    source1.OnNext(3);
                    source2.OnNext(1);
                }

                Assert.That(actual.Count, Is.EqualTo(1));
                Assert.That(actual[0], Is.EqualTo(expected));
            }
        }
    }
}
