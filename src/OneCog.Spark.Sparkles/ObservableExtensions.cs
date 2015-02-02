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
    }
}
