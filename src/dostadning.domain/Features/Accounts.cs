using System;
using System.Linq;
using System.Threading.Tasks;
using dostadning.domain.ourdata;
using dostadning.domain.result;

namespace dostadning.domain.features
{
    public interface IAccount
    {
        /// <summary>
        /// Request to become a user of dostadning
        /// </summary>
        /// <returns>A handle that can be used as a claim to dostadning features</returns>
        Task<Either<string>> Create();
    }

    public static class AccountFeature
    {
        static Error UnexpectedAffectCount => new DomainError("appuser.bad_affect_count");

        static Either<string> Acknowledge(int i, string id) => i == 1
            ? new Either<string>(id)
            : new Either<string>(UnexpectedAffectCount);
        public async static Task<Either<string>> Create(IRepository<Account, Guid> users)
        {
            var id = Guid.NewGuid();
            users.Add(new Account { Id = id });
            var i = await users.Commit();

            return i.FMap(x => Acknowledge(x, id.ToString()));
        }
    }
}