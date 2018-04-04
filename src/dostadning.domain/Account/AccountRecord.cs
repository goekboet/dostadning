using System;
using System.Collections.Generic;
using dostadning.domain.seller;

namespace dostadning.domain.account
{
    /// <summary>
    /// An Account represents a claim to dostadning features and data
    /// </summary>
    public class Account
    {
        /// <summary>
        /// The account id
        /// </summary>
        /// <returns>Id</returns>
        public Guid Id { get; set; }
        /// <summary>
        /// An Appuser can use dostadning to manage many tradera-users
        /// </summary>
        /// <returns>All the traderausers associated with this appuser</returns>
        public List<TraderaUser> TraderaUsers { get; set; } = new List<TraderaUser>();
    }
}