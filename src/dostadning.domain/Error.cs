namespace dostadning.domain
{
    public class Error : System.Exception
    {
        public Error() { }
        public Error(string message) : base(message) { }
        public Error(string message, System.Exception inner) : base(message, inner) { }
        protected Error(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}