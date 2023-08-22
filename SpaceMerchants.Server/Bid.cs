//-----------------------------------------------------------------------
// <copyright file="Bid.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Bid
    /// </summary>
    public struct Bid
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bid" /> struct.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="bidAmount">The bid amount.</param>
        /// <param name="wallet">The wallet to retrieve the funds from.</param>
        /// <param name="storage">The storage to transfer the cargo to upon winning.</param>
        public Bid(string item, int amount, int bidAmount, Wallet wallet, Storage storage)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(amount > 0);
            Contract.Requires(bidAmount > 0);
            Contract.Requires(wallet != null);
            Contract.Requires(storage != null);

            Item = item;
            Amount = amount;
            BidAmount = bidAmount;
            Wallet = wallet;
            Storage = storage;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public string Item { get; }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public int Amount { get; }

        /// <summary>
        /// Gets the bid amount.
        /// </summary>
        /// <value>The bid amount.</value>
        public int BidAmount { get; }

        /// <summary>
        /// Gets the wallet to retrieve the funds from.
        /// </summary>
        /// <value>The wallet.</value>
        public Wallet Wallet { get; }

        /// <summary>
        /// Gets the storage to transfer the cargo to upon winning.
        /// </summary>
        /// <value>The storage.</value>
        public Storage Storage { get; }
    }
}