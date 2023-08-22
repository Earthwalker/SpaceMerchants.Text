//-----------------------------------------------------------------------
// <copyright file="Cargo.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Cargo
    /// </summary>
    public class Storage
    {
        /// <summary>
        /// The items and amount.
        /// </summary>
        private Dictionary<string, int> items = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Storage"/> class.
        /// </summary>
        /// <param name="outpost">The outpost.</param>
        /// <param name="maxItems">The maximum items.</param>
        public Storage(Outpost outpost, int maxItems = -1)
        {
            Outpost = outpost;
            MaxItems = maxItems;
        }

        /// <summary>
        /// Gets or sets the outpost.
        /// </summary>
        /// <value>The outpost.</value>
        public Outpost Outpost { get; set; }

        /// <summary>
        /// Gets the maximum items that can be stored.
        /// </summary>
        /// <value>The maximum items.</value>
        public int MaxItems { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        public int Items
        {
            get
            {
                return items.Values.Sum();
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>The amount added.</returns>
        public int Add(string item, int amount)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(amount >= 0);

            if (MaxItems != -1)
                amount = Math.Min(MaxItems - Items, amount);

            if (!items.ContainsKey(item))
                items.Add(item, amount);
            else
                items[item] += amount;

            if (Contains(item) && items[item] < 0)
            {
                // DEBUG
            }

            return amount;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        public bool Remove(string item, int amount)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(amount >= 0);

            if (!Contains(item))
                return false;

            if (amount == items[item])
                items.Remove(item);
            else
                items[item] -= amount;

            if (Contains(item) && items[item] < 0)
            {
                // DEBUG
            }

            return true;
        }

        /// <summary>
        /// Determines whether this contains the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>Whether the item is contained here.</returns>
        public bool Contains(string item, int amount = 0)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(amount >= 0);

            return items.ContainsKey(item) && items[item] >= amount;
        }

        /// <summary>
        /// Determines whether this contains the specified amount of items.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public bool Contains(int amount = 0)
        {
            return Items > amount;
        }

        /// <summary>
        /// Gets the amount of the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The item amount.</returns>
        public int GetAmount(string item)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));

            if (items.ContainsKey(item))
                return items[item];

            return 0;
        }

        /// <summary>
        /// Transfers all the cargo to another <see cref="Storage"/>.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public int TransferCargo(Storage target)
        {
            Contract.Requires(target != null);

            // ensure we are at the same location
            if (Outpost != target.Outpost)
                return 0;

            int amount = 0;

            foreach (var item in items)
            {
                // add the cargo to the target
                int amountAdded = target.Add(item.Key, item.Value);

                if (amountAdded == 0)
                    break;
                else
                    amount += amountAdded;

                if (item.Value < 0)
                {
                    // DEBUG
                }
            }

            items.Clear();

            return amount;
        }

        /// <summary>
        /// Transfers the designated cargo to storage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>The amount of the cargo transferred.</returns>
        public int TransferCargo(Storage target, string item, int amount = 0)
        {
            Contract.Requires(target != null);
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(amount >= 0);

            // ensure we are at the same location
            if (Outpost != target.Outpost)
                return 0;

            // if amount has been set to 0, we should list the maximum amount
            if (amount == 0)
                amount = GetAmount(item);
            else
                amount = Math.Min(amount, GetAmount(item));

            if (amount == 0)
                return 0;

            // remove the cargo
            if (!Remove(item, amount))
            {
                target.Remove(item, amount);
                return 0;
            }

            // add the cargo to the target
            amount = target.Add(item, amount);

            return amount;
        }

        /// <summary>
        /// Gets to dictionary.
        /// </summary>
        /// <value>To dictionary.</value>
        public IReadOnlyDictionary<string, int> ToDictionary
        {
            get
            {
                return items;
            }
        }

        /// <summary>
        /// Gets the suggested value according to the pricing guide.
        /// </summary>
        /// <param name="outpost">The outpost we are are checking at.</param>
        /// <value>The suggested value.</value>
        public int SuggestedValue(Outpost outpost)
        {
            Contract.Requires(outpost != null);

            int value = 0;

            foreach (var item in items)
            {
                if (outpost != null)
                    value += outpost.GetLastSoldPrice(item.Key) * item.Value;
                else
                    value += Utility.StartingPrice() * item.Value;
            }

            return value;
        }
    }
}