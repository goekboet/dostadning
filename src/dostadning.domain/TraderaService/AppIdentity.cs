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
}