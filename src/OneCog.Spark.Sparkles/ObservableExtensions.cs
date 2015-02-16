using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OneCog.Spark.Sparkles
{
    public static class ObservableExtensions
    {
        public static IObservable<T> Retry<T>(this IObservable<T> source, IObserver<Exception> errors)
        {
            return Observable.Create<T>(
                observer =>
                {
                    Func<Exception, IObservable<T>> handler = null;

                    Func<IObservable<T>> caught = () => source.Catch(handler);

                    handler = exception =>
                    {
                        errors.OnNext(exception);
                        return caught();
                    };

                    return caught().Subscribe(observer);
                }
            );
        }

        public static IObservable<T> Retry<T>(this IObservable<T> source, Action<Exception> onError)
        {
            return Observable.Create<T>(
                observer =>
                {
                    Func<Exception, IObservable<T>> handler = null;

                    Func<IObservable<T>> caught = () => source.Catch(handler);

                    handler = exception =>
                    {
                        onError(exception);
                        return caught();
                    };

                    return caught().Subscribe(observer);
                }
            );
        }

        public static IObservable<TProjection> ZipLatest<T1,T2,TProjection>(this IObservable<T1> source1, IObservable<T2> source2, Func<T1,T2,TProjection> projection)
        {
            return Observable.Create<TProjection>(
                observer =>
                {
                    T1 value1 = default(T1);
                    bool got1 = false;
                    T2 value2 = default(T2);
                    bool got2 = false;

                    Action emit = () =>
                    {
                        if (got1 && got2)
                        {
                            observer.OnNext(projection(value1, value2));
                        }
                    };

                    Action<T1> on1 = x =>
                    {
                        value1 = x;
                        got1 = true;
                        emit();
                    };

                    Action<T2> on2 = x =>
                    {
                        value2 = x;
                        got2 = true;
                        emit();
                    };

                    return new CompositeDisposable(
                        source1.Subscribe(on1, observer.OnError, observer.OnCompleted),
                        source2.Subscribe(on2, observer.OnError, observer.OnCompleted)
                    );
                }
            );
        }
    }
}
