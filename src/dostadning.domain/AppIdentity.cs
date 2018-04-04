namespace dostadning.domain
{
    public sealed class AppIdentity
    {
        public AppIdentity(int id, string key, string pKey)
        {
            Id = id;
            Key = key;
            PKey = pKey;
        }
        public int Id { get; }
        public string Key { get; }

        public string PKey {get;}
    }
}