using System;

namespace dostadning.domain.result
{
    public sealed class Either<T>
    {
        public Either(T result) => Result = result;
        public Either(Error e) => Error = e;
        public bool IsError => Result?.Equals(default(T)) ?? true;
        public T Result { get; }
        public Error Error { get; }

        public Either<U> FMap<U>(Func<T, Either<U>> f) => IsError
                ? new Either<U>(Error)
                : f(Result);
    }
}