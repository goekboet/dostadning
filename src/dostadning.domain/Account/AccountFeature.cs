using System;
using System.Linq;
using System.Reactive.Linq;

namespace dostadning.domain.account
{
    public interface IAccount
    {
        /// <summary>
        /// Request to become a user of dostadning
        /// </summary>
        /// <returns>A handle that can be used as a claim to dostadning features</returns>
        IObservable<string> Create();
    }

    public static class AccountFeature
    {
        public static IObservable<string> Create(
            IRepository<Account, Guid> users) =>
            from id in Observable.Return(Guid.NewGuid())
            from _ in users.Add(new Account { Id = id }).Commit()
            select id.ToString();
    }
}