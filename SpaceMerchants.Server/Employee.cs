//-----------------------------------------------------------------------
// <copyright file="Employee.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Paid employee of a corporation.
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Employee"/> struct.
        /// </summary>
        /// <param name="outpost">The outpost.</param>
        public Employee(Outpost outpost)
        {
            Contract.Requires(outpost != null);

            Outpost = outpost;
            Wages = (int)outpost.Size * Utility.StartingPrice();
            Earnings = new Wallet(Outpost);
        }

        /// <summary>
        /// Gets the outpost.
        /// </summary>
        /// <value>The outpost.</value>
        public Outpost Outpost { get; }

        /// <summary>
        /// Gets or sets the wages.
        /// </summary>
        /// <value>The wages.</value>
        public int Wages { get; set; }

        /// <summary>
        /// Gets or sets the earnings.
        /// </summary>
        /// <value>The earnings.</value>
        public Wallet Earnings { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Employee"/> is profitable.
        /// </summary>
        /// <value><c>true</c> if profitable; otherwise, <c>false</c>.</value>
        public bool Profitable { get; set; }
    }
}