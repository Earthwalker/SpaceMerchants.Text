//-----------------------------------------------------------------------
// <copyright file="ServerAdmin.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Main menu.
    /// </summary>
    public enum MainMenuItem
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Saves the game.
        /// </summary>
        Save,

        /// <summary>
        /// Show debug data.
        /// </summary>
        Data,

        /// <summary>
        /// Exit the game.
        /// </summary>
        Exit,

        /// <summary>
        /// The number of items.
        /// </summary>
        Num
    }

    /// <summary>
    /// ServerAdmin
    /// </summary>
    public static class ServerAdmin
    {
		public static void ShowHelp()
		{
			// show menu items
            Game.WriteLine("1. (S)ave - Saves the world state (not functional)", MessageType.Default);
            Game.WriteLine("2. (D)ata - Show debug data", MessageType.Default);
            Game.WriteLine("3. (E)xit - Exits the current menu", MessageType.Default);
		}
		
        /// <summary>
        /// Main menu.
        /// </summary>
        /// <returns>Whether to continue to the next menu.</returns>
        public static void MainMenu()
        {
            string input = Console.ReadLine();

            switch (input.ToUpper())
            {
                // save
                case "SAVE":
                case "S":
                    SelectMainMenuItem(MainMenuItem.Save);
                    break;
                // debug data
                case "DATA":
                case "D":
                    SelectMainMenuItem(MainMenuItem.Data);
                    break;
                // exit the current menu
                case "EXIT":
                case "E":
                    SelectMainMenuItem(MainMenuItem.Exit);
                    break;

                default:
                    // check if the user entered an index
                    int index;
                    if (int.TryParse(input, out index) && index > 0 && index < (int)MainMenuItem.Num)
                        SelectMainMenuItem((MainMenuItem)index);
                    else
                        Game.WriteLine("Item not found", MessageType.Error);
                    break;
            } 
        }

		public static void SelectMainMenuItem(MainMenuItem selection)
		{
			switch (selection)
            {
                // save
                case MainMenuItem.Save:
                    /*Time.Pause();
                    Game.SaveGame.Corporations(CorporationDirectory);
                    Game.SaveGame.Outposts(OutpostDirectory);
                    Game.SaveGame.Ships(ShipDirectory);
                    Time.Start();*/
                    break;
                // debug data
                case MainMenuItem.Data:
                    Game.WriteLine();
					Game.WriteLine(Time.DateTime.ToString(), MessageType.Message);

                    //Time.Pause();

                    int totalShipBits = Game.Ships.Sum(s => s.Wallet.Bits);
                    int totalOutpostBits = Game.StarSystems.Sum(s => s.Planets.Sum(p => p.Outposts.Sum(o => o.Wallet.Bits)));
                    int totalMarketBits = Game.StarSystems.Sum(s => s.Planets.Sum(p => p.Outposts.Sum(o => o.MarketWallet.Bits)));
                    int totalBits = totalShipBits + totalOutpostBits + totalMarketBits;

                    int totalShipCargoWealth = Game.Ships.Sum(s => s.Cargo.SuggestedValue(s.Outpost));
                    int totalOutpostCargoWealth = Game.StarSystems.Sum(s => s.Planets.Sum(p => p.Outposts.Sum(o => o.Storage.SuggestedValue(o))));
                    int totalMarketCargoWealth = Game.StarSystems.Sum(s => s.Planets.Sum(p => p.Outposts.Sum(o => o.MarketStorage.SuggestedValue(o))));
                    int totalCargoWealth = totalShipCargoWealth + totalOutpostCargoWealth + totalMarketCargoWealth;

                    Game.WriteLine($"Total ship wealth: {totalShipBits} bits + {totalShipCargoWealth} cargo value ({totalShipBits + totalShipCargoWealth} total)");
                    Game.WriteLine($"Total outpost wealth: {totalOutpostBits} bits + {totalOutpostCargoWealth} cargo value ({totalOutpostBits + totalOutpostCargoWealth} total)");
                    Game.WriteLine($"Total market wealth: {totalMarketBits} bits + {totalMarketCargoWealth} cargo value ({totalMarketBits + totalMarketCargoWealth} total)");
                    Game.WriteLine($"Total wealth: {totalBits} bits + {totalCargoWealth} cargo value ({totalBits + totalCargoWealth} total)");

                    double averageShipBits = Game.Ships.Average(s => s.Wallet.Bits);
                    double averageShipCargoValue = Game.Ships.Average(s => s.Cargo.SuggestedValue(s.Outpost));

                    Game.WriteLine($"Average ship wealth: {averageShipBits} bits + {averageShipCargoValue} cargo value ({averageShipBits + averageShipCargoValue} total)");

                    double averageOutpostBits = Game.StarSystems.Average(s => s.Planets.Average(p => p.Outposts.Average(o => o.Wallet.Bits)));
                    double averageOutpostCargoValue = Game.StarSystems.Average(s => s.Planets.Average(p => p.Outposts.Average(o => o.Storage.SuggestedValue(o))));

                    Game.WriteLine($"Average outpost wealth: {averageOutpostBits} bits + {averageOutpostCargoValue} cargo value ({averageOutpostBits + averageOutpostCargoValue} total)");

                    var soldPrices = new List<int>();

                    foreach (var starSystem in Game.StarSystems)
                    {
                        foreach (var planet in starSystem.Planets)
                        {
                            foreach (var outpost in planet.Outposts)
                            {
                                foreach (var ledgerItem in outpost.MarketLedger)
                                    soldPrices.Add(ledgerItem.Price);
                            }
                        }
                    }

                    Game.WriteLine($"Average outpost item sell price: {(soldPrices.Count() > 0 ? soldPrices.Average() : -1)} bits");

                    //double averageCorporationBits = Game.Corporations.Average(c => c.Wallet.Bits);
                    //Game.WriteLine($"Average corporation wealth: {averageCorporationBits} bits");

                    var listings = Game.StarSystems.Sum(s => s.Planets.Sum(p => p.Outposts.Sum(o => o.OldListings.Count)));
                    Game.WriteLine($"Listings: {listings}");

                    if (listings > 0)
                        Game.WriteLine($"Average bid amount: {Game.StarSystems.Average(s => s.Planets.Average(p => p.Outposts.Average(o => o.Bids.DefaultIfEmpty().Average(b => b.BidAmount))))}");
                    else
                        Game.WriteLine($"Average bid amount: 0");
                    //Time.Start();
                    break;
                // exit the current menu
                case MainMenuItem.Exit:
                    Game.Server.Stop();
                    break;
            }
		}
    }
}