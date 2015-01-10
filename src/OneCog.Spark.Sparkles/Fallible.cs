using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneCog.Spark.Sparkles
{
    public class Fallible
    {
        public static Fallible<T> Success<T>(T value)
        {
            return new Fallible<T>(value);
        }

        public static Fallible<T> Fail<T>(Exception error)
        {
            return new Fallible<T>(error);
        }
    }

    public class Fallible<T>
    {
        public Fallible(T value)
        {
            Success = true;
            Value = value;
        }

        public Fallible(Exception error)
        {
            Success = false;
            Error = error;
        }

        public bool Success { get; private set; }

        public T Value { get; private set; }

        public Exception Error { get; private set; }
    }
}
