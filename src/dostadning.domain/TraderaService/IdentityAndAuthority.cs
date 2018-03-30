namespace dostadning.domain.result
{
    public sealed class AppIdentity
    {
        public AppIdentity(int id, string key)
        {
            Id = id;
            Key = key;
        }
        public int Id { get; }
        public string Key { get; }
    }

    public sealed class Consent
    {
        public Consent(
            int id,
            string token) 
            { Id = id; Token = token; }
            
        public int Id {get;}
        public string Token {get;}

        public override string ToString() => $"Consent for tradera user Id: {Id}";
    }
}