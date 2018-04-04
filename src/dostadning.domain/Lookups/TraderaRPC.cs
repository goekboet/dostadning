using System;
using System.Collections.Generic;
using System.Linq;

namespace dostadning.domain.lookups
{
    public sealed class Lookup : IEquatable<Lookup>
    {
        public Lookup(string k, IEnumerable<Constant> vs)
        {
            Key = k;
            Values = vs;
        }
        public string Key { get; } = string.Empty;
        public IEnumerable<Constant> Values { get; } = 
            Enumerable.Empty<Constant>();

        static string n => Environment.NewLine;
        static string table<T>(IEnumerable<T> es) => 
            string.Join(n, es.Select(x => x.ToString()));

        public override string ToString() =>
            $"{Key}:{n}" +
            $"{"ID", -6}{"Description", -50}{n}" +
            $"{table(Values)}{n}";

        public bool Equals(Lookup other) =>
            string.Equals(other.Key, Key);

        public override bool Equals(object obj) =>
            obj is Lookup l && Equals(l);

        public override int GetHashCode() => Key?.GetHashCode() ?? 0;

    }

    public sealed class Constant : IEquatable<Constant>
    {
        public Constant(int id, string desc)
        {
            Id = id;
            Description = desc;
        }

        public int Id { get; }
        public string Description { get; }
        
        public bool Equals(Constant other) =>
            other.Id == Id;

        public override bool Equals(object obj) =>
            obj is Constant c && Equals(c);

        public override int GetHashCode() => Id;

        public override string ToString() =>
            $"{Id,-6}{Description, -50}";
    }

    public interface ILookupCalls
    {
        IObservable<IEnumerable<Lookup>> GetItemRequestLookups();
        IObservable<IEnumerable<Lookup>> GetAcceptedBidderTypes();
        IObservable<IEnumerable<Lookup>> GetExpoItemTypes();
        IObservable<IEnumerable<Lookup>> GetItemTypes();

        IObservable<DateTime> ServerTime();
    }
}