//-----------------------------------------------------------------------
// <copyright file="Player.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants
{
    using System;
    using System.Linq;

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
    /// Main menu.
    /// </summary>
    public enum MainMenuItem
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Create new ship.
        /// </summary>
        Ship,

        /// <summary>
        /// Join server.
        /// </summary>
        Join,

        /// <summary>
        /// Exit the game.
        /// </summary>
        Exit,

        /// <summary>
        /// The number of items.
        /// </summary>
        Num
    }

    public enum ShipMenuItem
    {
        Name,

        Class
    }

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

    public class Player
    {
        public Player()
        {
            Local = true;
        }

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
        public string Name { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Player"/> is local.
        /// </summary>
        /// <value><c>true</c> if local; otherwise, <c>false</c>.</value>
        public bool Local { get; }

        /// <summary>
        /// Gets or sets the ship class.
        /// </summary>
        /// <value>The ship class.</value>
        public ShipClass ShipClass { get; set; }

        /// <summary>
        /// Gets the main menu selection.
        /// </summary>
        /// <value>The main menu selection.</value>
        public MainMenuItem MainMenuSelection { get; set; }

        /// <summary>
        /// Gets or sets the new menu selection.
        /// </summary>
        /// <value>The new menu selection.</value>
        public ShipMenuItem ShipMenuSelection { get; set; }

        /// <summary>
        /// Gets or sets the play menu selection.
        /// </summary>
        /// <value>The play menu selection.</value>
        public PlayMenuItem PlayMenuSelection { get; set; }

        /// <summary>
        /// Gets or sets the input.
        /// </summary>
        /// <value>The input.</value>
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we have data ready to send.
        /// </summary>
        /// <value><c>true</c> if we have data ready to send; otherwise, <c>false</c>.</value>
        public bool ReadyToSend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we have received a reply from the server.
        /// </summary>
        /// <value><c>true</c> if we've received a reply; otherwise, <c>false</c>.</value>
        public bool ReceivedReply { get; set; } = true;

        /// <summary>
        /// Main menu.
        /// </summary>
        /// <returns>Whether to continue to the next menu.</returns>
        public void MainMenu()
        {
            // show menu items
            Game.WriteLine($"1. (S)hip", MessageType.Default);

            if (!string.IsNullOrEmpty(Name) && ShipClass != ShipClass.None)
                Game.WriteLine($"2. (J)oin as {ShipClass} ship {Name}", MessageType.Default);
            else
                Game.WriteLine($"2. (J)oin", MessageType.Default);

            Game.WriteLine($"3. (E)xit", MessageType.Default);

            string input = Console.ReadLine();

            switch (input.ToUpper())
            {
                case "SHIP":
                case "S":
                    MainMenuSelection = MainMenuItem.Ship;
                    return;

                case "JOIN":
                case "J":
                    MainMenuSelection = MainMenuItem.Join;
                    return;

                default:
                    // check if the user wants to exit
                    if (Game.IsExitString(input.ToUpper()))
                    {
                        MainMenuSelection = MainMenuItem.Exit;
                        return;
                    }

                    // check if the user entered an index
                    int index;
                    if (int.TryParse(input, out index) && index > 0 && index < (int)MainMenuItem.Num)
                    {
                        MainMenuSelection = (MainMenuItem)index;
                        return;
                    }
                    break;
            }

            // show help
            Game.WriteLine("Invalid input", MessageType.Error);
        }

        /// <summary>
        /// New name menu.
        /// </summary>
        /// <returns>Whether to continue to the next menu.</returns>
        public bool ShipNameMenu()
        {
            // write saved name
            if (string.IsNullOrEmpty(Name))
                Game.WriteLine("Ship Name?", MessageType.Question);
            else
                Game.WriteLine($"Ship Name? (leave blank for {Name})", MessageType.Question);

            string input = Console.ReadLine().ToUpper();

            if (!string.IsNullOrEmpty(input))
            {
                // check if the user wants to exit the menu
                if (Game.IsExitString(input))
                {
                    MainMenuSelection = MainMenuItem.None;
                    return false;
                }

                Name = input;
                ShipMenuSelection = ShipMenuItem.Class;
                return true;
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                ShipMenuSelection = ShipMenuItem.Class;
                return true;
            }

            // show help
            Game.WriteLine("Name cannot be blank", MessageType.Error);

            return false;
        }

        /// <summary>
        /// Menu to select the ship class.
        /// </summary>
        /// <returns>Whether to continue to the next menu.</returns>
        public bool SelectShipClassMenu()
        {
            // write saved ship class
            if (ShipClass == ShipClass.None)
                Game.WriteLine("Ship Class?", MessageType.Question);
            else
                Game.WriteLine($"Ship Class? (leave blank for {ShipClass})", MessageType.Question);

            string input = Console.ReadLine().ToUpper();

            if (!string.IsNullOrEmpty(input))
            {
                // check if the user wants to exit the menu
                if (Game.IsExitString(input))
                {
                    MainMenuSelection = MainMenuItem.None;
                    return false;
                }

                // check if the user entered a valid class
                ShipClass shipClass;
                if (Enum.TryParse(input, true, out shipClass))
                {
                    ShipClass = shipClass;

                    return true;
                }

                // check if the user entered a valid class index
                int index;
                if (int.TryParse(input, out index) && index > 0 && index < (int)ShipClass.Num)
                {
                    ShipClass = (ShipClass)index;

                    return true;
                }
            }
            else if (ShipClass != ShipClass.None)
                return true;

            // show help
            Game.WriteLine("Ship class must be one of the following:", MessageType.Error);

            for (int i = 1; i < (int)ShipClass.Num; i++)
                Game.WriteLine($"{i}. {(ShipClass)i}");

            return false;
        }

        /// <summary>
        /// Menu to select the host.
        /// </summary>
        /// <returns>Whether to continue to the next menu.</returns>
        public bool SelectHostMenu()
        {
            // write saved host
            if (string.IsNullOrEmpty(Game.Host) && Game.Port <= 0)
                Game.WriteLine("Host?", MessageType.Question);
            else
                Game.WriteLine($"Host? (leave blank for {Game.Host}:{Game.Port})", MessageType.Question);

            string input = Console.ReadLine().ToUpper();

            if (!string.IsNullOrEmpty(input))
            {
                // check if the user wants to exit the menu
                if (Game.IsExitString(input))
                {
                    MainMenuSelection = MainMenuItem.None;
                    return false;
                }

                var splitInput = input.Split(':');
                Game.Host = splitInput.First();

                // check if the user entered a valid port number
                ushort port;
                if (ushort.TryParse(splitInput.Last(), out port))
                {
                    Game.Port = port;
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(Name) && Game.Port > 0)
                return true;

            // show help
            Game.WriteLine("Invalid host", MessageType.Error);

            return false;
        }

        /// <summary>
        /// Play menu.
        /// </summary>
        public void PlayMenu()
        {
            if (PlayMenuSelection == PlayMenuItem.Exit)
            {
                PlayMenuSelection = PlayMenuItem.None;
                ShowHelp();
            }

            string input = Console.ReadLine().ToUpper();

            if (string.IsNullOrEmpty(input))
                return;

            if (input.Length > 1)
            {
                Input = input;
                return;
            }

            var oldselection = PlayMenuSelection;

            switch (input.First())
            {
                // warp to another outpost
                case 'J':
                    PlayMenuSelection = PlayMenuItem.Warp;
                    break;
                // bid
                case 'B':
                    PlayMenuSelection = PlayMenuItem.Bid;
                    break;
                // create listing
                case 'L':
                    PlayMenuSelection = PlayMenuItem.List;
                    break;
                // list cargo
                case 'C':
                    PlayMenuSelection = PlayMenuItem.Cargo;
                    break;
                // show news
                case 'N':
                    PlayMenuSelection = PlayMenuItem.News;
                    break;
                // show outpost info
                case 'O':
                    PlayMenuSelection = PlayMenuItem.Outpost;
                    break;
                // exit the game
                case 'E':
                    if (PlayMenuSelection == PlayMenuItem.None)
                        MainMenuSelection = MainMenuItem.None;
                    PlayMenuSelection = PlayMenuItem.Exit;
                    break;
                // show help
                case 'H':
                    ShowHelp();
                    break;

                default:
                    Input = input;
                    return;
            }

            if (oldselection != PlayMenuItem.None && PlayMenuSelection == oldselection)
                Input = "R";
            else
                Input = string.Empty;
        }

        /// <summary>
        /// Shows the help.
        /// </summary>
        public void ShowHelp()
        {
            Game.WriteLine("(J)ump - Warp to a location", MessageType.Default);
            Game.WriteLine("(B)id - Bid on available listings", MessageType.Default);
            Game.WriteLine("(L)ist - List an item", MessageType.Default);
            Game.WriteLine("(C)argo - Show cargo", MessageType.Default);
            Game.WriteLine("(N)ews - Show the news", MessageType.Default);
            Game.WriteLine("(O)utpost - Show outpost info", MessageType.Default);
            Game.WriteLine("(E)xit - Exits the current menu", MessageType.Default);
            Game.WriteLine("(H)elp - Show help", MessageType.Default);
        }
    }
}