//-----------------------------------------------------------------------
// <copyright file="LedgerItem.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Holds data about transactions for a particular item.
    /// </summary>
    public class LedgerItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LedgerItem"/> struct.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="price">The price.</param>
        public LedgerItem(string item, int price = 0)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));

            Item = item;
            Price = price;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        public string Item { get; }

        /// <summary>
        /// Gets the number sold.
        /// </summary>
        /// <value>The number sold.</value>
        public int NumberTraded { get; private set; }

        /// <summary>
        /// Gets the average price.
        /// </summary>
        /// <value>The average price.</value>
        public int Price { get; private set; }

        /// <summary>
        /// Adds the specified price.
        /// </summary>
        /// <param name="price">The price.</param>
        public void Add(int price)
        {
            Contract.Requires(price >= 0);

            if (Price == 0)
                Price = price;
            else
                Price = ((Price * NumberTraded) + price) / (NumberTraded + 1);

            NumberTraded++;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            return Item;
        }
    }
}