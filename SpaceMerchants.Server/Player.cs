//-----------------------------------------------------------------------
// <copyright file="Player.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Play menu.
    /// </summary>
    public enum PlayMenuItem
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Warp to another outpost.
        /// </summary>
        Warp,

        /// <summary>
        /// View listings to bid on.
        /// </summary>
        Bid,

        /// <summary>
        /// Create listing.
        /// </summary>
        List,

        /// <summary>
        /// List cargo.
        /// </summary>
        Cargo,

        /// <summary>
        /// Show news.
        /// </summary>
        News,

        /// <summary>
        /// Show outpost info.
        /// </summary>
        Outpost,

        /// <summary>
        /// Exit current menu.
        /// </summary>
        Exit,
    }

    /// <summary>
    /// Player which implements <see cref="IPlayer"/>.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="local">if set to <c>true</c> we are local.</param>
        public Player(string name, bool local)
        {
            Name = name;
            Local = local;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Player"/> is local.
        /// </summary>
        /// <value><c>true</c> if local; otherwise, <c>false</c>.</value>
        public bool Local { get; }

        /// <summary>
        /// Gets or sets the ship.
        /// </summary>
        /// <value>The ship.</value>
        public Ship Ship { get; set; }

        private StarSystem selectedStarSystem;

        private Planet selectedPlanet;

        private Outpost selectedOutpost;

        /// <summary>
        /// Gets or sets the item type selection.
        /// </summary>
        /// <value>The item type selection.</value>
        private string selectedItemType;

        /// <summary>
        /// Gets or sets the item to be listed.
        /// </summary>
        /// <value>The item to be listed.</value>
        private string selectedItem;

        /// <summary>
        /// Gets or sets the amount to be listed.
        /// </summary>
        /// <value>The amount to be listed.</value>
        private int selectedAmount;

        private int selectedIndex;

        /// <summary>
        /// The selected main menu item.
        /// </summary>
        private PlayMenuItem playMenuSelection = PlayMenuItem.None;

        /// <summary>
        /// Gets or sets the input.
        /// </summary>
        /// <value>The input.</value>
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the replies.
        /// </summary>
        /// <value>The replies.</value>
        public Dictionary<string, MessageType> Replies { get; } = new Dictionary<string, MessageType>();

        /// <summary>
        /// Gets or sets the replies.
        /// </summary>
        /// <value>The replies.</value>
        public Dictionary<string, MessageType> OldReplies { get; } = new Dictionary<string, MessageType>();


        /// <summary>
        /// Use menu.
        /// </summary>
        /// <param name="menuItem">The menu item.</param>
        public void UseMenu(PlayMenuItem menuItem)
        {
            if (menuItem != playMenuSelection || Input == "R")
            {
                // reset everything
                selectedStarSystem = null;
                selectedPlanet = null;
                selectedOutpost = null;
                selectedItemType = string.Empty;
                selectedItem = string.Empty;
                selectedAmount = -1;
                selectedIndex = -1;

                if (Input == "R")
                {
                    Input = string.Empty;
                    playMenuSelection = PlayMenuItem.None;
                }
            }

            Dictionary<string, MessageType> replies;

            switch (menuItem)
            {
                case PlayMenuItem.Warp:
                    replies = WarpMenu();
                    break;

                case PlayMenuItem.Bid:
                    replies = BidMenu();
                    break;

                case PlayMenuItem.List:
                    replies = ListMenu();
                    break;

                case PlayMenuItem.Cargo:
                    replies = CargoMenu();
                    break;

                case PlayMenuItem.News:
                    replies = NewsMenu();
                    break;

                case PlayMenuItem.Outpost:
                    replies = OutpostMenu();
                    break;

                default:
                    return;
            }

            playMenuSelection = menuItem;

            foreach (var reply in replies)
                Replies.Add(reply.Key, reply.Value);

            if (Replies.Count == 0)
                Replies.Add("Invalid input", MessageType.Error);

            if (Replies.Count(r => r.Value == MessageType.Error) == Replies.Count)
                Replies.AddRange(OldReplies.Where(r => r.Value != MessageType.Error));
        }

        /// <summary>
        /// Warp menu.
        /// </summary>
        /// <returns>Reply to the player.</returns>
        public Dictionary<string, MessageType> WarpMenu()
        {
            var reply = new Dictionary<string, MessageType>();

            if (playMenuSelection != PlayMenuItem.Warp)
            {
                for (int i = 0; i < Game.StarSystems.Count; i++)
                    reply.Add($"{i + 1}. {Game.StarSystems[i].Name}", MessageType.Default);

                reply.Add($"Star System? (leave blank for {Ship.Outpost.Planet.StarSystem.Name})", MessageType.Question);
            }
            else if (selectedStarSystem == null)
            {
                // select star system
                if (string.IsNullOrEmpty(Input))
                    selectedStarSystem = Ship.Outpost.Planet.StarSystem;
                else
                    selectedStarSystem = Game.StarSystems.Find(s => s.Name.ToUpper() == Input);

                if (selectedStarSystem == null)
                {
                    // check if the player entered a number
                    int index;
                    if (int.TryParse(Input, out index) && index > 0 && index <= Game.StarSystems.Count)
                        selectedStarSystem = Game.StarSystems[index - 1];
                }

                if (selectedStarSystem != null)
                {
                    reply.Add($"{selectedStarSystem.Name} selected", MessageType.Question);

                    for (int i = 0; i < selectedStarSystem.Planets.Count; i++)
                        reply.Add($"{i + 1}. {selectedStarSystem.Planets[i].Name}", MessageType.Default);

                    reply.Add($"Planet? (leave blank for {Ship.Outpost.Planet.Name})", MessageType.Question);
                }
                else
                    reply.Add("Star System not found", MessageType.Error);
            }
            else if (selectedPlanet == null)
            {
                // select planet
                if (string.IsNullOrEmpty(Input))
                    selectedPlanet = Ship.Outpost.Planet;
                else
                    selectedPlanet = selectedStarSystem.Planets.Find(p => p.Name.ToUpper() == Input);

                if (selectedPlanet == null)
                {
                    // check if the player entered a number
                    int index;
                    if (int.TryParse(Input, out index) && index > 0 && index <= selectedStarSystem.Planets.Count)
                        selectedPlanet = selectedStarSystem.Planets[index - 1];
                }

                if (selectedPlanet != null)
                {
                    reply.Add($"{selectedPlanet.Name} selected", MessageType.Question);

                    for (int i = 0; i < selectedPlanet.Outposts.Count; i++)
                        reply.Add($"{i + 1}. {selectedPlanet.Outposts[i].Size} {selectedPlanet.Outposts[i].Name}", MessageType.Default);

                    reply.Add($"Outpost?", MessageType.Question);
                }
                else
                    reply.Add("Planet not found", MessageType.Error);
            }
            else if (selectedOutpost == null)
            {
                // select outpost
                selectedOutpost = selectedPlanet.Outposts.Find(o => o.Name.ToUpper() == Input);

                if (selectedOutpost == null)
                {
                    // check if the player entered a number
                    int index;
                    if (int.TryParse(Input, out index) && index > 0 && index <= selectedPlanet.Outposts.Count)
                        selectedOutpost = selectedPlanet.Outposts[index - 1];
                }

                if (selectedOutpost != null)
                {
                    Ship.Warp(selectedOutpost);

                    reply.Add($"Warped to {selectedOutpost.Name}", MessageType.Message);

                    UseMenu(PlayMenuItem.None);
                }
                else
                    reply.Add("Outpost not found", MessageType.Error);
            }

            return reply;
        }

        /// <summary>
        /// Bid menu.
        /// </summary>
        /// <returns>Reply to the player.</returns>
        public Dictionary<string, MessageType> BidMenu()
        {
            var reply = new Dictionary<string, MessageType>();

            var listings = Ship.Outpost.OldListings.FindAll(l => l.OwnerWallet != Ship.Wallet);

            if (playMenuSelection != PlayMenuItem.Bid)
            {
                // show available bits
                reply.Add($"Bits: {Ship.Wallet.Bits}", MessageType.Default);

                // show weight
                reply.Add($"Weight: {Ship.Cargo.Items}/{Ship.Cargo.MaxItems}", MessageType.Default);

                reply.Add($"Listings: {listings.Count()}", MessageType.Default);

                for (int i = 0; i < Game.ItemTypes.Count; i++)
                {
                    var listingsOfItemType = listings.FindAll(l => l.Item.StartsWith(Game.ItemTypes[i]));
                    reply.Add($"{i + 1}. {Game.ItemTypes[i]}: {listingsOfItemType.Count()}({listingsOfItemType.Count})", MessageType.Default);
                }

                reply.Add("Item type?", MessageType.Question);

                // set our selected item type to nothing
                selectedItemType = string.Empty;
            }
            else if (string.IsNullOrEmpty(selectedItemType))
            {
                // get item type
                if (!Game.ItemTypes.Exists(i => i.ToUpper() == Input))
                {
                    // check if the player entered a number
                    int index;
                    if (int.TryParse(Input, out index) && index > 0 && index <= Game.ItemTypes.Count)
                    {
                        selectedItemType = Game.ItemTypes[index - 1];
                        selectedItem = string.Empty;

                        // narrow the listings down to the specified type
                        listings = listings.Where(l => l.Item.StartsWith(selectedItemType, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        listings.Sort((x, y) => x.Item.CompareTo(y.Item));

                        // list remaining listings TODO: remove dupes?
                        for (int i = 0; i < listings.Count; i++)
                            reply.Add($"{i + 1}. {listings[i].Item.Split('.').Last()}", MessageType.Default);

                        reply.Add("Item to buy?", MessageType.Question);
                    }
                    else
                        reply.Add("Item type not found", MessageType.Error);
                }
                else
                    reply.Add("Item to buy?", MessageType.Question);
            }
            else if (string.IsNullOrEmpty(selectedItem))
            {
                // narrow the listings down to the specified type
                listings = listings.Where(l => l.Item.StartsWith(selectedItemType, StringComparison.CurrentCultureIgnoreCase)).ToList();
                listings.Sort((x, y) => x.Item.CompareTo(y.Item));

                // parse input
                int index;
                if (int.TryParse(Input, out index) && index > 0 && index <= listings.Count)
                {
                    selectedItem = listings[index - 1].Item;
                    reply.Add($"Amount to buy? ({Ship.Cargo.GetAmount(selectedItem)} in cargo)", MessageType.Question);
                    selectedAmount = -1;
                }
                else
                    reply.Add("Item not found", MessageType.Error);
            }
            // get amount to buy
            else if (selectedAmount == -1)
            {
                if (string.IsNullOrEmpty(Input))
                    selectedAmount = 1;
                else
                    int.TryParse(Input, out selectedAmount);

                if (selectedAmount >= 0)
                    reply.Add($"Starting bid amount per item (Leave empty for last sold price)?", MessageType.Question);
                else
                    reply.Add("Must be a number 0 or larger", MessageType.Error);
            }
            else
            {
                // parse input
                int bidAmount = -1;

                if (string.IsNullOrEmpty(Input))
                    bidAmount = Ship.Outpost.GetLastSoldPrice(selectedItem);
                else
                    int.TryParse(Input, out bidAmount);

                if (bidAmount > 0)
                {
                    Ship.Outpost.Bids.Add(new Bid(selectedItem, selectedAmount, bidAmount, Ship.Wallet, Ship.Cargo, this));
                    reply.Add($"Bid of {bidAmount} bits placed for {selectedAmount} {selectedItem.Split('.').Last()}", MessageType.Message);

                    // return to buy menu
                    Input = string.Empty;
                    playMenuSelection = PlayMenuItem.None;
                    var innerReply = BidMenu();

                    foreach (var message in innerReply)
                        reply.Add(message.Key, message.Value);
                }
                else
                    reply.Add("Bid amount must be larger than 0", MessageType.Error);
            }

            return reply;
        }

        /// <summary>
        /// List items menu.
        /// </summary>
        /// <returns>Reply to the player.</returns>
        public Dictionary<string, MessageType> ListMenu()
        {
            var reply = new Dictionary<string, MessageType>();

            // sort the ship's cargo
            var sortedCargo = Ship.Cargo.ToDictionary.Keys.ToList();
            sortedCargo.Sort();

            // list items in cargo
            if (playMenuSelection != PlayMenuItem.List)
            {
                reply.Add("Item to list?", MessageType.Question);

                string line;
                List<Listing> existingListings;

                for (int i = 0; i < sortedCargo.Count; i++)
                {
                    line = $"{i + 1}. {sortedCargo[i].Split('.').Last()}({Ship.Cargo.GetAmount(sortedCargo[i])})";

                    // append amount listed if it's being listed
                    existingListings = Ship.Outpost.OldListings.FindAll(l => l.Item == sortedCargo[i]);

                    line += $"({existingListings.Count} listed)";

                    reply.Add(line, MessageType.Default);
                }

                // reset selected item
                selectedItem = string.Empty;
            }
            // get listing item
            else if (string.IsNullOrEmpty(selectedItem))
            {
                if (!sortedCargo.Exists(i => i.ToUpper() == Input))
                {
                    int index;

                    if (int.TryParse(Input, out index) && index > 0 && index <= Ship.Cargo.Items)
                        selectedItem = sortedCargo[index - 1];
                    else
                        reply.Add("Item not found", MessageType.Default);
                }
                else
                    selectedItem = Input;

                if (!string.IsNullOrEmpty(selectedItem))
                {
                    reply.Add($"Amount to list? ({Ship.Cargo.GetAmount(selectedItem)})", MessageType.Question);

                    // reset selected amount
                    selectedAmount = -1;
                }
            }
            // get listing amount
            else if (selectedAmount == -1)
            {
                if (string.IsNullOrEmpty(Input))
                    selectedAmount = 0;
                else
                    int.TryParse(Input, out selectedAmount);

                if (selectedAmount >= 0)
                    reply.Add($"Starting bid amount per item (0 for suggested)?", MessageType.Question);
                else
                    reply.Add("Must be a number 0 or larger", MessageType.Error);
            }
            // get listing price
            else
            {
                int startingBid = -1;

                if (string.IsNullOrEmpty(Input))
                    startingBid = 0;
                else
                    int.TryParse(Input, out startingBid);

                if (startingBid >= 0)
                {
                    var listing = Ship.Outpost.CreateListing(selectedItem, selectedAmount, Ship.Cargo, startingBid, Ship.Wallet, this);

                    if (listing != null)
                    {
                        reply.Add($"Listed {listing.Count} {listing.First().Item.Split('.').Last()} with a starting bid of {startingBid} bits each ({listing.Count * startingBid})", MessageType.Message);

                        // return to listing menu
                        Input = string.Empty;
                        playMenuSelection = PlayMenuItem.None;
                        UseMenu(PlayMenuItem.List);
                    }
                    else
                        reply.Add("Something went wrong", MessageType.Error);
                }
                else
                    reply.Add("Must be a number 0 or larger", MessageType.Error);
            }

            return reply;
        }

        /// <summary>
        /// Show cargo menu.
        /// </summary>
        /// <returns>Reply to the player.</returns>
        public Dictionary<string, MessageType> CargoMenu()
        {
            var reply = new Dictionary<string, MessageType>();

            // sort cargo
            var sortedCargo = Ship.Cargo.ToDictionary.Keys.ToList();
            sortedCargo.Sort();

            // find warehouses we have access to
            var warehouses = new List<int>();

            foreach (var item in sortedCargo)
            {
                if (item.StartsWith($"Other.{Ship.Outpost.Name} Warehouse"))
                    warehouses.Add(int.Parse(item.Split(' ').Last()));
            }
            
            // calculate total items available to choose
            int totalItems = sortedCargo.Count;

            foreach (var warehouse in warehouses)
                totalItems += Ship.Outpost.Warehouses[warehouse].Items;

            // show sorted cargo
            if (playMenuSelection != PlayMenuItem.Cargo)
            {
                // show bits
                reply.Add($"Bits: {Ship.Wallet.Bits}", MessageType.Default);
                reply.Add($"Weight: {Ship.Cargo.Items}/{Ship.Cargo.MaxItems}", MessageType.Default);

                // list sorted cargo
                for (int i = 0; i < sortedCargo.Count; i++)
                    reply.Add($"{i + 1}. {sortedCargo[i]}: {Ship.Cargo.GetAmount(sortedCargo[i])}", MessageType.Default);

                int itemNumber = sortedCargo.Count;

                // sort warehouse items
                List<string> sortedWarehouseCargo;

                // list warehouses
                foreach (var warehouse in warehouses)
                {
                    sortedWarehouseCargo = Ship.Outpost.Warehouses[warehouse].ToDictionary.Keys.ToList();
                    sortedWarehouseCargo.Sort();

                    reply.Add($"Warehouse {warehouse}: {sortedWarehouseCargo.Count}", MessageType.Default);

                    for (int i = 0; i < sortedWarehouseCargo.Count; i++)
                        reply.Add($"{itemNumber + i + 1}. {sortedCargo[i]}: {Ship.Outpost.Warehouses[warehouse].GetAmount(sortedWarehouseCargo[i])}", MessageType.Default);
                }

                reply.Add("Item to select?", MessageType.Question);
            }
            else if (string.IsNullOrEmpty(selectedItem))
            {
                if (!sortedCargo.Exists(i => i.ToUpper() == Input))
                {
                    int index;

                    if (int.TryParse(Input, out index) && index > 0 && index <= totalItems)
                    {
                        if (index <= sortedCargo.Count)
                        {
                            selectedItem = sortedCargo[index - 1];
                            selectedIndex = -1;
                        }
                        else
                        {
                            index -= sortedCargo.Count;

                            for (int i = 0; i < warehouses.Count; i++)
                            {
                                if (index <= Ship.Outpost.Warehouses[warehouses[i]].Items)
                                {
                                    var sortedWarehouseCargo = Ship.Outpost.Warehouses[warehouses[i]].ToDictionary.Keys.ToList();
                                    sortedWarehouseCargo.Sort();

                                    selectedItem = sortedWarehouseCargo[index - 1];
                                    selectedIndex = i;
                                    break;
                                }
                                else
                                    index -= Ship.Outpost.Warehouses[warehouses[i]].Items;
                            }
                        }
                    }
                    else
                        reply.Add("Item not found", MessageType.Error);
                }
                else
                    selectedItem = Input;

                if (!string.IsNullOrEmpty(selectedItem))
                {
                    reply.Add($"1. Use {selectedItem.Split('.').Last()}", MessageType.Default);

                    if (warehouses.Count > 0)
                    {
                        if (selectedIndex == -1)
                        {
                            for (int i = 0; i < warehouses.Count; i++)
                                reply.Add($"{i + 2}. Transfer to warehouse {warehouses[i]}({Ship.Outpost.Warehouses[warehouses[i]].Items}/{Outpost.WarehouseSpace})", MessageType.Default);
                        }
                        else
                            reply.Add($"2. Transfer to cargo", MessageType.Default);
                    }

                    reply.Add($"Action? ({Ship.Cargo.GetAmount(selectedItem)})", MessageType.Question);
                }
            }
            else
            {
                int index = -1;
                if (!int.TryParse(Input, out index) || index <= 0 || index >= warehouses.Count + 1)
                {
                    reply.Add("Action not found", MessageType.Error);
                    return reply;
                }

                // use item
                if (index == 1)
                {
                    // TODO: use item
                }
                // transfer items to or from warehouses
                else
                {
                    // transfer from warehouse to cargo
                    if (index == 2 && selectedIndex > -1)
                        reply.Add($"Moved {Ship.Outpost.Warehouses[warehouses[selectedIndex]].TransferCargo(Ship.Cargo, selectedItem, 0)} {selectedItem.Split('.').Last()} from warehouse", MessageType.Message);
                    // transfer from cargo to warehouse
                    else
                        reply.Add($"Moved {Ship.Cargo.TransferCargo(Ship.Outpost.Warehouses[warehouses[index - 1]], selectedItem, 0)} {selectedItem.Split('.').Last()} to warehouse {warehouses[index - 1]}", MessageType.Message);                        
                }

                // return to cargo menu
                Input = string.Empty;
                playMenuSelection = PlayMenuItem.None;
                UseMenu(PlayMenuItem.Cargo);
            }

            return reply;
        }

        /// <summary>
        /// Show news menu.
        /// </summary>
        /// <returns>Reply to the player.</returns>
        public Dictionary<string, MessageType> NewsMenu()
        {
            var reply = new Dictionary<string, MessageType>();

            reply.Add($"News for: {Time.DateTime.DayOfWeek}", MessageType.Default);

            bool noNews = true;

            // add headlines from the outposts on the same planet
            foreach (var outpost in Ship.Outpost.Planet.Outposts)
            {
                if (!string.IsNullOrEmpty(outpost.CurrentHeadline))
                {
                    reply.Add($"{outpost.CurrentHeadline.Split('.').Last()} at {outpost.Name}", MessageType.Default);
                    noNews = false;
                }
            }

            // no news
            if (noNews)
                reply.Add("No current headlines", MessageType.Default);

            // return to main menu
            UseMenu(PlayMenuItem.None);

            return reply;
        }

        /// <summary>
        /// Show outpost info.
        /// </summary>
        /// <returns>Reply to the player.</returns>
        public Dictionary<string, MessageType> OutpostMenu()
        {
            var reply = new Dictionary<string, MessageType>();

            reply.Add($"Outpost: {Ship.Outpost.Name}", MessageType.Default);
            reply.Add($"Planet: {Ship.Outpost.Planet.Name}", MessageType.Default);
            reply.Add($"StarSystem: {Ship.Outpost.Planet.StarSystem.Name}", MessageType.Default);

            var ships = new List<Ship>();
            ships.AddRange(Ship.Outpost.Ships);

            reply.Add($"Ships: {ships.Count}", MessageType.Default);

            foreach (var ship in ships)
                reply.Add($"{ship.ShipClass} ship {ship.Name}", MessageType.Default);

            // return to main menu
            UseMenu(PlayMenuItem.None);

            return reply;
        }
    }
}