using System;

namespace dostadning.domain.result
{
    public abstract class Error
    {
        protected Error(string code) => Code = code;
        public string Code { get; }
    }

    public sealed class DomainError : Error
    {
        public DomainError(string msg)
            : base(msg) { }
    }

    public sealed class ExceptionalError : Error
    {
        public ExceptionalError(Exception e, string msg)
            : base(msg) { Exception = e; }
        public Exception Exception { get; }
    }
}