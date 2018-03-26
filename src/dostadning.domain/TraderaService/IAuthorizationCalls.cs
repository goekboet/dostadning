using System;
using System.Threading.Tasks;
using dostadning.domain.result;

namespace dostadning.domain.service.tradera
{
    public sealed class Token
    {
        public Token(string id, DateTimeOffset exp)
        {
            Id = id;
            Expires = exp;
        }
        public string Id { get; }
        public DateTimeOffset Expires { get; }
    }

    public interface IAuthorizationCalls
    {
        /// <summary>
        /// Fetch the integer Id that is associated with a tradera-alias.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns>The Id</returns>
        IObservable<int> Identify(string alias);

        /// <summary>
        /// Fetch the token that represents a user consent for dostadning to manipulate its tradera-account
        /// </summary>
        /// <param name="traderaUserId"></param>
        /// <param name="requestId"></param>
        /// <returns>token and a time of expiration</returns>
        IObservable<Token> FetchToken(
            int traderaUserId,
            string requestId);
    }
}