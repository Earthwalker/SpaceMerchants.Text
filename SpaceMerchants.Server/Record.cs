//-----------------------------------------------------------------------
// <copyright file="Record.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Record
    /// </summary>
    public class Record
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Record" /> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="outpost">The outpost.</param>
        /// <param name="prices">The prices.</param>
        /// <param name="volumes">The volumes.</param>
        public Record(DateTime dateTime, Outpost outpost, List<int> prices, List<int> volumes)
        {
            DateTime = dateTime;
            Outpost = outpost;
            Prices = prices;
            Volumes = volumes;
        }

        public DateTime DateTime { get; }

        public Outpost Outpost { get; }

        public List<int> Prices { get; }

        public List<int> Volumes { get; }
    }
}
