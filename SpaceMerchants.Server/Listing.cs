//-----------------------------------------------------------------------
// <copyright file="Listing.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Listing
    /// </summary>
    public struct Listing
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Listing" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="ownerWallet">The wallet.</param>
        /// <param name="storage">The storage.</param>
        public Listing(string item, Wallet ownerWallet, Storage storage, int startingBid)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(ownerWallet != null);
            Contract.Requires(storage != null);

            Item = item;
            OwnerWallet = ownerWallet;
            Storage = storage;
            StartingBid = startingBid;
        }

        /// <summary>
        /// Gets the owner wallet.
        /// </summary>
        /// <value>The owner wallet.</value>
        public Wallet OwnerWallet { get; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        public string Item { get; }

        /// <summary>
        /// Gets the storage.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        public Storage Storage { get; }

        public int StartingBid { get; }
    }
}