//-----------------------------------------------------------------------
// <copyright file="Outpost.cs" company="Leamware">
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
    /// Determines the traffic to this outpost.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutpostSize
    {
        /// <summary>
        /// Out-of-the-way outpost.
        /// </summary>
        Small,

        /// <summary>
        /// Normal-size outpost.
        /// </summary>
        Medium,

        /// <summary>
        /// Densely-populated outpost.
        /// </summary>
        Large,

        /// <summary>
        /// Size number.
        /// </summary>
        Num
    }

    /// <summary>
    /// Outpost
    /// </summary>
    public class Outpost : Entity
    {
        /// <summary>
        /// The minimum angle allowed between neighbors.
        /// </summary>
        public static int MinNeighborAngle = 30;

        /// <summary>
        /// The maximum neighbor distance.
        /// </summary>
        public static int MaxNeighborDistance = 20;

        /// <summary>
        /// The amount of space in each warehouse.
        /// </summary>
        public static int WarehouseSpace = 20;

        /// <summary>
        /// The warehouse size multiplier.
        /// </summary>
        public const int WarehouseSizeMultiplier = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Outpost"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="planet">The planet.</param>
        /// <param name="size">The size.</param>
        /// <param name="mainExportItemType">The main export item type.</param>
        public Outpost(string name, Planet planet, OutpostSize size, string mainExportItemType)
            : base(name)
        {
            Contract.Requires(planet != null);
            Contract.Requires(size >= OutpostSize.Small && size < OutpostSize.Num);
            Contract.Requires(!string.IsNullOrEmpty(mainExportItemType));

            Planet = planet;
            Size = size;
            Planet.Outposts.Add(this);
            MainExportItemType = mainExportItemType;

            // add to updates
            Time.TickEvent += Update;

            Storage = new Storage(this);
            MarketStorage = new Storage(this);
            Wallet = new Wallet(this);
            MarketWallet = new Wallet(this);

            // DEBUG: add 1000 bit
            Wallet.AddBits(1000);

            // add warehouse space deeds to cargo
            for (int i = 0; i < (int)Size * WarehouseSizeMultiplier; i++)
            {
                Storage.Add($"Other.{Name} Warehouse {i + 1}", 1);
                Warehouses.Add(new Storage(this, WarehouseSpace));
            }

            // populate popularity index with item types
            foreach (var itemType in Game.ItemTypes)
                PopularityIndex.Add(itemType, 0);

            ProductionRate = (int)Size;
        }

        /// <summary>
        /// Gets the planet.
        /// </summary>
        /// <value>The planet.</value>
        public Planet Planet { get; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        public OutpostSize Size { get; }

        /// <summary>
        /// Gets the main export item type.
        /// </summary>
        /// <value>The main export item type.</value>
        public string MainExportItemType { get; }

        /// <summary>
        /// Gets the cargo.
        /// </summary>
        /// <value>The cargo.</value>
        public Storage Storage { get; }

        /// <summary>
        /// Gets the market storage.
        /// </summary>
        /// <value>The market storage.</value>
        public Storage MarketStorage { get; }

        /// <summary>
        /// Gets the wallet.
        /// </summary>
        /// <value>The wallet.</value>
        public Wallet Wallet { get; private set; }

        /// <summary>
        /// Gets the market wallet.
        /// </summary>
        /// <value>The market wallet.</value>
        public Wallet MarketWallet { get; }

        /// <summary>
        /// Gets the ledger of all the transactions done at this outpost.
        /// </summary>
        public List<Transaction> MarketLedger { get; } = new List<Transaction>();

        /// <summary>
        /// Gets the pricing guide.
        /// </summary>
        /// <value>The pricing guide.</value>
        public List<LedgerItem> PricingGuide { get; } = new List<LedgerItem>();

        /// <summary>
        /// Gets the old listings.
        /// </summary>
        /// <value>The old listings.</value>
        public List<Listing> OldListings { get; } = new List<Listing>();

        /// <summary>
        /// Gets the new listings.
        /// </summary>
        /// <value>The new listings.</value>
        public List<Listing> NewListings { get; } = new List<Listing>();

        /// <summary>
        /// Gets the bids.
        /// </summary>
        /// <value>
        /// The bids.
        /// </value>
        public List<Bid> Bids { get; } = new List<SpaceMerchants.Server.Bid>();

        /// <summary>
        /// Gets the warehouses.
        /// </summary>
        /// <value>The warehouses.</value>
        public List<Storage> Warehouses { get; } = new List<Storage>();

        /// <summary>
        /// Gets the popularity index for items.
        /// </summary>
        /// <value>The popularity index for items.</value>
        public Dictionary<string, int> PopularityIndex { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets the production rate.
        /// </summary>
        /// <value>The production rate.</value>
        public int ProductionRate { get; private set; }

        /// <summary>
        /// Gets the ships currently at this outpost.
        /// </summary>
        /// <value>The ships.</value>
        [JsonIgnore]
        public IReadOnlyList<Ship> Ships
        {
            get
            {
                return Game.Ships.FindAll(s => s.Outpost == this);
            }
        }

        /// <summary>
        /// Gets the current headline.
        /// </summary>
        /// <value>The current headline.</value>
        public string CurrentHeadline { get; private set; }

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
        /// <param name="planet">The planet.</param>
        /// <returns>The generated outpost.</returns>
        public static Outpost Generate(Planet planet)
        {
            Contract.Requires(planet != null);

            string name;
            string[] splitModifier;

            while (true)
            {
                // pick a name
                name = Game.LocationNames.Pick();

                // pick a modifier
                splitModifier = Game.Modifiers.Pick().Split('.');

                // add modifier if it matches
                if (splitModifier.First() == "Location")
                    name += ' ' + splitModifier.Last();

                // ensure the name is unique on this planet
                if (!planet.Outposts.Exists(o => o.Name == name))
                {
                    // create the new outpost
                    return new Outpost(name, planet, (OutpostSize)Utility.RNG.Next(1, (int)OutpostSize.Num), Game.ItemTypes.Pick());
                }
            }
        }

        /// <summary>
        /// Creates a new listing.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="storage">The storage.</param>
        /// <param name="startingBidAmount">The price.</param>
        /// <param name="wallet">The wallet.</param>
        /// <returns>The new listing.</returns>
        public List<Listing> CreateListing(string item, int amount, Storage storage, int startingBidAmount, Wallet wallet)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(amount >= 0);
            Contract.Requires(storage != null);
            Contract.Requires(startingBidAmount >= 0);
            Contract.Requires(wallet != null);

            // amount of 0 means max available
            if (amount == 0)
                amount = storage.GetAmount(item);

            if (amount == 0)
                return null;

            // move the cargo to the outpost
            amount = storage.TransferCargo(MarketStorage, item, amount);

            if (amount == 0)
                return null;

            // if price is 0, use pricing guide
            if (startingBidAmount == 0)
                startingBidAmount = GetSuggestedValue(item);

            var listings = new List<Listing>();

            for (int i = 0; i < amount; i++)
                listings.Add(new Listing(item, wallet, storage, startingBidAmount));

            NewListings.AddRange(listings);

            // TODO: starting bid amount
            //Bids.Add(new Bid(item, amount, startingBidAmount, wallet, storage));

            return listings;
        }

        /// <summary>
        /// Finds the winners of the biddings.
        /// </summary>
        private void FindBidWinners()
        {
            var winningBids = new List<KeyValuePair<string, int>>();
            int amount = 0;
            var marketStorage = MarketStorage.ToDictionary.ToList();
            int marketBits;
            List<string> debugMessages = new List<string>();

            foreach (var item in marketStorage)
            {
                if (Bids.Count == 0)
                    continue;

                // get all bids for this item
                var bids = Bids.FindAll(b => b.Item == item.Key);

                if (bids.Count == 0)
                    continue;

                // shuffle so the first ones don't always win tie bids
                bids.Shuffle();

                // sort bids highest to lowest
                bids.Sort((x, y) => y.BidAmount.CompareTo(x.BidAmount));

                // get winning bids
                amount = item.Value;
                int amountBought;
                foreach (var bid in bids)
                {
                    if (amount == 0)
                        break;

                    // get amount the bidder can afford
                    amountBought = Math.Min(amount, Math.Min(bid.Amount, (int)Math.Floor((double)bid.Wallet.Bits / bid.BidAmount)));

                    if (amountBought == 0)
                        continue;

                    debugMessages.Add($"new bid");

                    // transfer the cargo
                    amountBought = MarketStorage.TransferCargo(bid.Storage, item.Key, amountBought);

                    if (amountBought == 0)
                        continue;

                    debugMessages.Add($"amount bought: {amountBought}");

                    if (!bid.Wallet.TransferBits(MarketWallet, bid.BidAmount * amountBought))
                    {
                        bid.Storage.TransferCargo(MarketStorage, item.Key, amountBought);
                        debugMessages.Add($"transfered cargo");
                        continue;
                    }

                    for (int i = 0; i < amountBought; i++)
                    {
                        winningBids.Add(new KeyValuePair<string, int>(item.Key, bid.BidAmount));

                        MarketLedger.Add(new Transaction(null, bid.Wallet, item.Key, bid.BidAmount));

                        if (bid.BidAmount < 100)
                        {
                            // DEBUG
                        }

                        debugMessages.Add($"record: {i}");
                    }

                    amount -= amountBought;

                    // if the winner is the outpost, things are handled differently
                    if (bid.Storage == Storage)
                    {
                        debugMessages.Add($"storage");
                        // lower popularity index since we've satisfied the demand
                        PopularityIndex[item.Key.Split('.').First()] -= amountBought;// -= (int)(.01 * PopularityIndex.Sum(i => i.Value)) * amountBought;

                        // remove the items from our storage unless we posted it ourselves or it's a
                        // share or warehouse key
                        if (!item.Key.EndsWith("Warehouse"))
                            Storage.Remove(item.Key, amountBought);
                    }

                    break;
                }

                // pay listers
                var listers = OldListings.FindAll(l => l.Item == item.Key);
                listers.Shuffle();

                marketBits = MarketWallet.Bits;

                var winningBidsOfItem = winningBids.FindAll(b => b.Key == item.Key);

                debugMessages.Add($"winning bids: {winningBidsOfItem.Count}");
                debugMessages.Add($"listers: {listers.Count}");

                amount = item.Value;
                foreach (var lister in listers)
                {
                    debugMessages.Add($"amount: {amount}");

                    if (amount == 0)
                        break;

                    // Make sure the winning bidder still has enough funds.
                    if (!MarketWallet.TransferBits(lister.OwnerWallet, winningBidsOfItem.FirstOrDefault().Value))
                        continue;

                    debugMessages.Add($"got money: {winningBidsOfItem.FirstOrDefault().Value}");

                    if (winningBidsOfItem.Count > 0)
                        winningBidsOfItem.RemoveAt(0);

                    OldListings.Remove(lister);

                    // update records
                    var oldRecord = Records.Find(r => r.Outpost == this);
                }
            }
            
            // transfer leftovers to outpost
            //MarketStorage.TransferCargo(Storage);

            if (MarketWallet.Bits > 0)
            {
                // DEBUG
            }

            /*var winningBids = new List<Bid>();

            foreach (var listing in OldListings)
            {
                winningBids.Clear();

                foreach (var bid in listing.Value)
                {
                    // ensure the bid isn't null
                    if (bid.Wallet == null || bid.BidAmount <= 0)
                        continue;

                    if (bid.Wallet.Bits >= bid.BidAmount)
                    {
                        // clear the other winners if we have more than all of them
                        if (winningBids.Count > 0 && bid.BidAmount > winningBids.First().BidAmount)
                            winningBids.Clear();

                        // add the bid to the winner if it's high enough
                        if (winningBids.Count == 0 || bid.BidAmount >= winningBids.First().BidAmount)
                            winningBids.Add(bid);
                    }
                }

                // if no winners were chosen, the lister has the winning bid
                if (winningBids.Count == 0)
                    winningBids.Add(listing.Value.First());

                // pick a winner if multiple winners and transfer the items
                var winningBid = winningBids.Pick();

                // transfer the bid amount
                winningBid.Wallet.TransferBits(listing.Key.OwnerWallet, winningBid.BidAmount * listing.Key.Amount);

                int amount = MarketStorage.TransferCargo(winningBid.Storage, listing.Key.Item, listing.Key.Amount);

                // if the winner is the outpost, things are handled differently
                if (winningBid.Storage == Storage)
                {
                    // lower popularity index since we've satisfied the demand
                    PopularityIndex[listing.Key.Item.Split('.').First()] -= (int)(.01 * PopularityIndex.Sum(i => i.Value));

                    // remove the items from our storage unless we posted it ourselves or it's a
                    // share or warehouse key
                    var splitItem = listing.Key.Item.Split(' ');
                    if (splitItem.Length != 3 || splitItem[splitItem.Length - 2] != "Share")
                    {
                        if (winningBid.Wallet != listing.Key.OwnerWallet && !listing.Key.Item.EndsWith("Warehouse"))
                            Storage.Remove(listing.Key.Item, amount);
                    }
                }

                // if the winner doesn't have enough space, the outpost gets the extras
                if (amount < listing.Key.Amount)
                    MarketStorage.TransferCargo(Storage, listing.Key.Item, listing.Key.Amount - amount);

                // if the owner bought it from himself, lower the value by 10%
                if (winningBid.Wallet == listing.Key.OwnerWallet)
                    UpdatePricingGuide(listing.Key.Item, (int)(winningBid.BidAmount * .9));
                else
                    UpdatePricingGuide(listing.Key.Item, winningBid.BidAmount);
            }*/

            // clear the old listings
            //OldListings.Clear();

            // move the new listings to the old
            OldListings.AddRange(NewListings);

            NewListings.Clear();
        }

        /// <summary>
        /// Generates the news.
        /// </summary>
        public void GenerateNews()
        {
            string newHeadline = string.Empty;

            do
            {
                newHeadline = Game.Headlines.Pick();
            }
            while (CurrentHeadline == newHeadline);

            CurrentHeadline = newHeadline;
        }

        /// <summary>
        /// Gets the suggested value for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The price.</returns>
        public int GetSuggestedValue(string item)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));

            if (!PricingGuide.Exists(i => i.Item == item))
            {
                var newItem = new LedgerItem(item);
                PricingGuide.Add(newItem);
                newItem.Add(Utility.StartingPrice());
                return newItem.Price;
            }

            return PricingGuide.Find(i => i.Item == item).Price;
        }

        /// <summary>
        /// Updates the pricing guide.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        public void UpdatePricingGuide(string item, int value)
        {
            Contract.Requires(!string.IsNullOrEmpty(item));
            Contract.Requires(value >= 0);

            if (!PricingGuide.Exists(i => i.Item == item))
            {
                var newItem = new LedgerItem(item);
                PricingGuide.Add(newItem);
                newItem.Add(Utility.StartingPrice());
                newItem.Add(value);
            }
            else
                PricingGuide.Find(i => i.Item == item).Add(value);
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
        /// Gets the popularity multiplier.
        /// </summary>
        /// <param name="itemType">Type of the item.</param>
        /// <returns>The multiplier.</returns>
        private double GetPopularityMultiplier(string itemType)
        {
            Contract.Requires(!string.IsNullOrEmpty(itemType));

            if (!PopularityIndex.ContainsKey(itemType))
                PopularityIndex.Add(itemType, 0);

            double result = 1;
            int sign = Math.Sign(PopularityIndex[itemType]);

            if (PopularityIndex[itemType] > 0)
            {
                for (int i = 1; i < PopularityIndex[itemType]; i++)
                    result *= 1.1;
            }
            else if (PopularityIndex[itemType] < 0)
            {
                for (int i = 1; i < Math.Abs(PopularityIndex[itemType]); i++)
                    result *= .9;
            }

            return result;
        }

        /// <summary>
        /// AI updates.
        /// </summary>
        public void Update(object sender, EventArgs args)
        {
            // generate local news and refresh listings at midnight
            if (Time.DateTime.Hour == 0)
            {
                FindBidWinners();

                // generate news weekly
                if (Time.DateTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    GenerateNews();

                    // headlines affect production
                    switch (CurrentHeadline.Split('.').First())
                    {
                        case "Good":
                            ProductionRate++;

                            if (ProductionRate > (int)Size * 2)
                                ProductionRate = (int)Size * 2;
                            break;

                        case "Bad":
                            ProductionRate--;

                            if (ProductionRate < 0)
                                ProductionRate = 0;
                            break;

                        case "Neutral":
                            ProductionRate += Math.Sign((int)Size - ProductionRate);
                            break;
                    }

                    // weight certain industries depending on economy state?

                    // update popularity index
                    int points = Utility.RNG.Next(0, (int)Size + 1);

                    // choose item types to increase popularity
                    for (int i = 0; i < points; i++)
                        PopularityIndex[Game.ItemTypes.Pick()]++;

                    // generate a new item an add it to the outpost storage
                    if (ProductionRate > 0)
                    {
                        string item = Game.GenerateItem(MainExportItemType);
                        Storage.Add(item, ProductionRate);
                    }
                }
            }
            
            // list shares, warehouse space, and products at 6am
            if (Time.DateTime.Hour == 6)
            {
                // list products in storage
                if (Storage.Contains())
                {
                    var storage = Storage.ToDictionary.ToList();

                    foreach (var item in storage)
                        CreateListing(item.Key, Utility.RNG.Next(item.Value), Storage, (int)(GetSuggestedValue(item.Key) * .9), Wallet);
                }
            }

            // bid for products at 6pm
            if (Time.DateTime.Hour == 18)
            {
                double bidAmount;

                foreach (var listing in OldListings)
                {
                    if (listing.OwnerWallet == Wallet)
                        continue;

                    // with no minimum bids, don't even place bids if not popular
                    //if (GetPopularityMultiplier(listing.Item.Split('.').First()) < 1)
                    //    continue;

                    bidAmount = Math.Max(1, GetPopularityMultiplier(listing.Item.Split('.').First())) * GetSuggestedValue(listing.Item);

                    //Bids.Add(new Bid(listing.Item, Math.Max(1, PopularityIndex[listing.Item.Split('.').First()]), (int)bidAmount, wallet, Storage));
                    Bids.Add(new Bid(listing.Item, 10, 100, Wallet, Storage));
                }
            }
        }
    }
}