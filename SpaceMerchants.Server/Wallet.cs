//-----------------------------------------------------------------------
// <copyright file="Wallet.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.Contracts;

namespace SpaceMerchants.Server
{
    /// <summary>
    /// Wallet
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// The bits.
        /// </summary>
        private int bits;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wallet"/> class.
        /// </summary>
        /// <param name="outpost">The outpost.</param>
        public Wallet(Outpost outpost)
        {
            Outpost = outpost;
        }

        /// <summary>
        /// Gets or sets the outpost.
        /// </summary>
        /// <value>The outpost.</value>
        public Outpost Outpost { get; set; }

        /// <summary>
        /// Gets or sets the bits.
        /// </summary>
        /// <value>The bits.</value>
        public int Bits
        {
            get
            {
                return bits;
            }
        }

        /// <summary>
        /// Transfers the bits. 0 for all.
        /// </summary>
        /// <param name="wallet">The target.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>Whether the transfer was successful.</returns>
        public bool TransferBits(Wallet wallet, int amount = 0)
        {
            Contract.Requires(wallet != null);
            Contract.Requires(amount >= 0);

            // amount being 0 means all
            if (amount == 0)
                amount = bits;

            //ensure we have enough bits
            if (bits >= amount)
            {
                wallet.bits += amount;
                bits -= amount;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Adds the bits.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void AddBits(int amount)
        {
            bits += amount;
        }
    }
}