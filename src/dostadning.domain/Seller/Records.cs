using System;
using System.Collections.Generic;
using dostadning.domain.auction.record;

namespace dostadning.domain.seller
{
    /// <summary>
    /// A traderauser
    /// </summary>
    public class TraderaUser
    {
        /// <summary>
        /// Every traderauser has a string alias
        /// </summary>
        /// <returns>The alias</returns>
        public string Alias { get; set; }
        /// <summary>
        /// Every traderauser also has an integer id. Some calls to the soap service needs this as input. 
        /// We can query the soap api with an alias and get the associated id and deduce that there is such
        /// an alias with tradera. 
        /// </summary>
        /// <returns>The Id</returns>
        public int Id { get; set; }
        /// <summary>
        /// Status of the consent we have been given for this traderauser.
        /// </summary>
        /// <returns>The consent</returns>
        public TraderaConsent Consent { get; set; }

        public List<Auction> Auctions { get; set; }
    }

    /// <summary>
    /// Represents a Request for an Appuser to give dostadning consent to interact with tradera on behalf of them
    /// </summary>
    public class TraderaConsent
    {
        /// <summary>
        /// A handle on the request that is shared with Tradera
        /// </summary>
        /// <returns>The id</returns>
        public Guid Id { get; set; }
        /// <summary>
        /// If the user has given consent we can fetch a token from tradera to authenticate calls. We store this
        /// token to not have to make redundant calls.
        /// </summary>
        /// <returns>The token</returns>
        public string Token { get; set; }
        /// <summary>
        /// Is null if Token is not set otherwise the token expiration time.
        /// </summary>
        /// <returns>The time of expiration</returns>
        public DateTimeOffset? Expires { get; set; }
    }
}