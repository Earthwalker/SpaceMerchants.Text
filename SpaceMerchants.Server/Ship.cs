//-----------------------------------------------------------------------
// <copyright file="Ship.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The class of the <see cref="Ship"/> determines the weight it can hold.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShipClass
    {
        /// <summary>
        /// No class.
        /// </summary>
        None,

        /// <summary>
        /// Designed for traveling.
        /// </summary>
        Light,

        /// <summary>
        /// Balanced between traveling and volume trading.
        /// </summary>
        Medium,

        /// <summary>
        /// Designed for volume trading.
        /// </summary>
        Heavy,

        /// <summary>
        /// The number of ship classes.
        /// </summary>
        Num
    }

    /// <summary>
    /// Ship
    /// </summary>
    public class Ship : Entity
    {
        private Outpost outpost;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ship"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="outpost">The outpost.</param>
        /// <param name="shipClass">The ship class.</param>
        /// <param name="human">Whether this ship is controlled by a human.</param>
        public Ship(string name, Outpost outpost, ShipClass shipClass, bool human)
            : base(name)
        {
            Contract.Requires(outpost != null);
            Contract.Requires(shipClass > ShipClass.None && shipClass < ShipClass.Num);

            this.outpost = outpost;
            ShipClass = shipClass;

            Human = human;

            // add to updates
            if (!Human)
                Time.TickEvent += Update;

            Cargo = new Storage(Outpost, (int)ShipClass * Game.MaxWeightMultiplier);
            Wallet = new Wallet(Outpost);

            // DEBUG: add 1000 bit
            Wallet.AddBits(1000);
        }

        /// <summary>
        /// Gets or sets the outpost.
        /// </summary>
        /// <value>The outpost.</value>
        public Outpost Outpost
        {
            get
            {
                return outpost;
            }

            set
            {
                outpost = value;
                Cargo.Outpost = value;
                Wallet.Outpost = value;
            }
        }

        /// <summary>
        /// Gets the ship class.
        /// </summary>
        /// <value>The ship class.</value>
        public ShipClass ShipClass { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Ship"/> is controlled by a human.
        /// </summary>
        /// <value><c>true</c> if human; otherwise, <c>false</c>.</value>
        public bool Human { get; }

        /// <summary>
        /// Gets the cargo.
        /// </summary>
        /// <value>The cargo.</value>
        public Storage Cargo { get; }

        /// <summary>
        /// Gets the wallet.
        /// </summary>
        /// <value>The wallet.</value>
        public Wallet Wallet { get; }

        /// <summary>
        /// Gets the records.
        /// </summary>
        /// <value>
        /// The records.
        /// </value>
        public List<Record> Records { get; } = new List<Record>();

        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <returns>The generated ship.</returns>
        public static Ship Generate()
        {
            string name;
            string[] splitModifier;

            while (true)
            {
                // pick a name
                name = Game.ShipNames.Pick();

                // pick a modifier
                splitModifier = Game.Modifiers.Pick().Split('.');

                // add modifier if it matches
                if (splitModifier.First() == "Ship")
                    name += ' ' + splitModifier.Last();

                // ensure the name is unique in this galaxy
                if (!Game.Ships.Exists(s => s.Name == name))
                {
                    // create the new ship
                    return new Ship(name, Utility.PickOutpost(), (ShipClass)Utility.RNG.Next((int)ShipClass.None + 1, (int)ShipClass.Num), false);
                }
            }
        }

        /// <summary>
        /// Warps to the specified <see cref="Outpost"/>.
        /// </summary>
        /// <param name="outpost">The outpost.</param>
        public void Warp(Outpost outpost = null)
        {
            if (outpost == null)
                outpost = Utility.PickOutpost();

            if (Outpost == outpost)
                return;

            // update records from old outpost if any
            if (Outpost != null)
                UpdateRecords(Outpost.Records);

            Outpost = outpost;

            // check if there's any dividends waiting here
            if (Outpost.Corporation?.Headquarters == Outpost)
            {
                foreach (var item in Cargo.ToDictionary.Keys)
                {
                    if (item.StartsWith($"Other.{Outpost.Corporation.Name} Share"))
                    {
                        int bits = Wallet.Bits;

                        if (Outpost.Corporation.ShareholderWallets[int.Parse(item.Split(' ').Last())].TransferBits(Wallet))
                        {
                            if (Human)
                                Game.SendMessage(this, $"Received {Wallet.Bits - bits} bits from {Outpost.Corporation.Name} shares");
                        }
                    }
                }
            }

            // update records if the outpost has more recent records
            Outpost.UpdateRecords(Records);
        }

        /// <summary>
        /// Updates the records.
        /// </summary>
        /// <param name="records">The records.</param>
        public void UpdateRecords(List<Record> records)
        {
            Record localRecord;

            foreach (var record in records)
            {
                localRecord = Records.Find(r => r.Outpost == record.Outpost);

                if (localRecord.Outpost == null)
                    Records.Add(record);
                else
                {
                    if (record.DateTime > localRecord.DateTime)
                    {
                        // remove old record
                        Records.Remove(localRecord);

                        // add new record
                        Records.Add(record);
                    }
                }
            }
        }

        /// <summary>
        /// AI updates.
        /// </summary>
        public void Update(object sender, EventArgs args)
        {
            // warp to an outpost if we're not at one
            if (Outpost == null)
            {
                Warp();
                return;
            }

            // list products at 6am
            if (Time.DateTime.Hour == 6 && !Outpost.OldListings.Exists(l => l.OwnerWallet == Wallet))
            {
                if (Utility.RNG.Next(10) == 0)
                    Warp();

                if (Cargo.Contains())
                {
                    var cargo = Cargo.ToDictionary.ToList();

                    foreach (var item in cargo)
                    {
                        var startingBid = Utility.RNG.NextDouble(1, 1.2, 4) * Outpost.GetSuggestedValue(item.Key);

                        Outpost.CreateListing(item.Key, Utility.RNG.Next(item.Value), Cargo, (int)startingBid, Wallet);
                    }
                }
            }

            // buy products from 6am to 6pm
            if (Time.DateTime.Hour == 6 && Time.DateTime.Hour <= 18)
            {
                double bidAmount;

                // check if we have enough space for this if we win
                if (Cargo.Items < Cargo.MaxItems)
                {
                    foreach (var listing in Outpost.OldListings)
                    {
                        bidAmount = Utility.RNG.NextDouble(.8, 1, 4) * Outpost.GetSuggestedValue(listing.Item);

                        // TODO: set amount we want based on popularity
                        Outpost.Bids.Add(new Bid(listing.Item, Utility.RNG.Next(1, 5), (int)100, Wallet, Cargo));
                    }
                }
            }
        }
    }
}