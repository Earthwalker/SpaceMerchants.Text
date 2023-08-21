//-----------------------------------------------------------------------
// <copyright file="FileIO.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Handles saving and loading.
    /// </summary>
    public static class FileIO
    {
        /// <summary>
        /// The directory where the star systems are stored.
        /// </summary>
        public const string StarSystemDirectory = @"\Star Systems\";

        /// <summary>
        /// The directory where the ships are stored.
        /// </summary>
        public const string ShipDirectory = @"\Ships\";

        /// <summary>
        /// The settings file name.
        /// </summary>
        public const string SettingsFileName = @"\settings.cfg";

        /// <summary>
        /// The headlines file name.
        /// </summary>
        public const string HeadlinesFileName = @"\Headlines.txt";

        /// <summary>
        /// The modifiers file name.
        /// </summary>
        public const string ModifiersFileName = @"\Modifiers.txt";

        /// <summary>
        /// The item names file name.
        /// </summary>
        public const string ItemNamesFileName = @"\ItemNames.txt";

        /// <summary>
        /// The ship names file name.
        /// </summary>
        public const string ShipNamesFileName = @"\ShipNames.txt";

        /// <summary>
        /// The outpost names file name.
        /// </summary>
        public const string LocationNamesFileName = @"\LocationNames.txt";

        /// <summary>
        /// Loads everything.
        /// </summary>
        public static void LoadAll()
        {
            LoadSettings();
            LoadModifiers();
            LoadItems();
            LoadHeadlines();
            LoadLocationNames();
            LoadShipNames();
            LoadStarSystems();
            LoadShips();
        }

        /// <summary>
        /// Saves everything.
        /// </summary>
        public static void SaveAll()
        {
            SaveStarSystems();
            SaveShips();
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        public static void LoadSettings()
        {
            // ensure the file exists
            if (!File.Exists(Environment.CurrentDirectory + SettingsFileName))
                return;

            var lines = File.ReadAllLines(Environment.CurrentDirectory + SettingsFileName);
            string[] splitLine;

            // loop through all lines in the file, setting the values accordingly
            foreach (var line in lines)
            {
                splitLine = line.Split('=');

                switch (splitLine.First().ToUpper())
                {
                    case "NAME":
                        Game.Name = splitLine.Last().ToUpper();
                        break;

                    case "MAXPLAYERS":
                        int maxPlayers;
                        if (int.TryParse(splitLine.Last(), out maxPlayers))
                            Game.MaxPlayers = maxPlayers;
                        break;

                    case "PORT":
                        int port;
                        if (int.TryParse(splitLine.Last(), out port))
                            Game.Port = port;
                        break;

                    case "KEY":
                        Game.ConnectionKey = splitLine.Last();
                        break;

                    case "HOURSPERTICK":
                        int hoursPerTick;
                        if (int.TryParse(splitLine.Last(), out hoursPerTick))
                            Game.HoursPerTick = hoursPerTick;
                        break;

                    case "TICKINTERVAL":
                        int tickInterval;
                        if (int.TryParse(splitLine.Last(), out tickInterval))
                            Game.TickInterval = tickInterval;
                        break;

                    case "CORPORATIONS":
                        int corporations;
                        if (int.TryParse(splitLine.Last(), out corporations))
                            Game.CorporationsToGenerate = corporations;
                        break;

                    case "STARSYSTEMS":
                        int starSystems;
                        if (int.TryParse(splitLine.Last(), out starSystems))
                            Game.StarSystemsToGenerate = starSystems;
                        break;

                    case "MINPLANETSPERSYSTEM":
                        int minPlanetsPerSystem;
                        if (int.TryParse(splitLine.Last(), out minPlanetsPerSystem))
                            Game.MinPlanetsPerSystem = minPlanetsPerSystem;
                        break;

                    case "MAXPLANETSPERSYSTEM":
                        int maxPlanetsPerSystem;
                        if (int.TryParse(splitLine.Last(), out maxPlanetsPerSystem))
                            Game.MaxPlanetsPerSystem = maxPlanetsPerSystem;
                        break;

                    case "MINOUTPOSTSPERPLANET":
                        int minOutpostsPerPlanet;
                        if (int.TryParse(splitLine.Last(), out minOutpostsPerPlanet))
                            Game.MinOutpostsPerPlanet = minOutpostsPerPlanet;
                        break;

                    case "MAXOUTPOSTSPERPLANET":
                        int maxOutpostsPerPlanet;
                        if (int.TryParse(splitLine.Last(), out maxOutpostsPerPlanet))
                            Game.MaxOutpostsPerPlanet = maxOutpostsPerPlanet;
                        break;

                    case "SHIPS":
                        int ships;
                        if (int.TryParse(splitLine.Last(), out ships))
                            Game.ShipsToGenerate = ships;
                        break;

                    case "DEFAULTCOLOR":
                        ConsoleColor defaultColor;

                        if (Enum.TryParse(splitLine.Last(), out defaultColor))
                            Game.Colors.Add(MessageType.Default, defaultColor);
                        break;

                    case "QUESTIONCOLOR":
                        ConsoleColor questionColor;

                        if (Enum.TryParse(splitLine.Last(), out questionColor))
                            Game.Colors.Add(MessageType.Question, questionColor);
                        break;

                    case "MESSAGECOLOR":
                        ConsoleColor messageColor;

                        if (Enum.TryParse(splitLine.Last(), out messageColor))
                            Game.Colors.Add(MessageType.Message, messageColor);
                        break;

                    case "ERRORCOLOR":
                        ConsoleColor errorColor;

                        if (Enum.TryParse(splitLine.Last(), out errorColor))
                            Game.Colors.Add(MessageType.Error, errorColor);
                        break;

                    case "INPUTCOLOR":
                        ConsoleColor inputColor;

                        if (Enum.TryParse(splitLine.Last(), out inputColor))
                            Game.Colors.Add(MessageType.Input, inputColor);
                        break;
                }
            }
        }

        /// <summary>
        /// Loads the star systems.
        /// </summary>
        public static void LoadStarSystems()
        {
            // ensure the directory exists
            if (!Directory.Exists(Environment.CurrentDirectory + StarSystemDirectory))
                return;

            // clear existing star systems
            Game.StarSystems.Clear();

            foreach (var fileName in Directory.EnumerateFiles(Environment.CurrentDirectory + StarSystemDirectory))
                Game.StarSystems.Add(JsonConvert.DeserializeObject<StarSystem>(File.ReadAllText(fileName)));

            Game.WriteLine($"Loaded {Game.StarSystems.Count} star systems", MessageType.Message);
        }

        /// <summary>
        /// Saves the outposts.
        /// </summary>
        public static void SaveStarSystems()
        {
            // if the directory doesn't exist, create it
            if (!Directory.Exists(Environment.CurrentDirectory + StarSystemDirectory))
                Directory.CreateDirectory(Environment.CurrentDirectory + StarSystemDirectory);

            foreach (var starSystem in Game.StarSystems)
                File.WriteAllText(Environment.CurrentDirectory + StarSystemDirectory + starSystem.Name + ".json", JsonConvert.SerializeObject(starSystem));

            Game.WriteLine($"Saved {Game.StarSystems.Count} star systems", MessageType.Message);
        }

        /// <summary>
        /// Loads the ships.
        /// </summary>
        public static void LoadShips()
        {
            // ensure the directory exists
            if (!Directory.Exists(Environment.CurrentDirectory + ShipDirectory))
                return;

            // clear existing ships
            Game.Ships.Clear();

            foreach (var fileName in Directory.EnumerateFiles(Environment.CurrentDirectory + ShipDirectory))
                JsonConvert.DeserializeObject<Ship>(File.ReadAllText(fileName));

            Game.WriteLine($"Loaded {Game.Ships.Count} ships", MessageType.Message);
        }

        /// <summary>
        /// Loads the item types.
        /// </summary>
        public static void LoadItems()
        {
            // ensure the file exists
            if (!File.Exists(Environment.CurrentDirectory + ItemNamesFileName))
                return;

            // clear existing item types and item names
            Game.ItemTypes.Clear();
            Game.ItemNames.Clear();

            Game.ItemNames.AddRange(File.ReadAllLines(Environment.CurrentDirectory + ItemNamesFileName));

            foreach (var item in Game.ItemNames)
            {
                if (!Game.ItemTypes.Contains(item.Split('.').First()))
                    Game.ItemTypes.Add(item.Split('.').First());
            }
        }

        /// <summary>
        /// Loads the location names.
        /// </summary>
        public static void LoadLocationNames()
        {
            // ensure the file exists
            if (!File.Exists(Environment.CurrentDirectory + LocationNamesFileName))
                return;

            // clear existing outpost names
            Game.LocationNames.Clear();

            // read file
            Game.LocationNames.AddRange(File.ReadAllLines(Environment.CurrentDirectory + LocationNamesFileName));
        }

        /// <summary>
        /// Loads the ship names.
        /// </summary>
        public static void LoadShipNames()
        {
            // ensure the file exists
            if (!File.Exists(Environment.CurrentDirectory + ShipNamesFileName))
                return;

            // clear existing ship names
            Game.ShipNames.Clear();

            // read file
            Game.ShipNames.AddRange(File.ReadAllLines(Environment.CurrentDirectory + ShipNamesFileName));
        }

        /// <summary>
        /// Loads the headlines.
        /// </summary>
        public static void LoadHeadlines()
        {
            // ensure the file exists
            if (!File.Exists(Environment.CurrentDirectory + HeadlinesFileName))
                return;

            // clear existing headlines
            Game.Headlines.Clear();

            // read file
            Game.Headlines.AddRange(File.ReadAllLines(Environment.CurrentDirectory + HeadlinesFileName));
        }

        /// <summary>
        /// Loads the modifiers.
        /// </summary>
        public static void LoadModifiers()
        {
            // ensure the file exists
            if (!File.Exists(Environment.CurrentDirectory + ModifiersFileName))
                return;

            // clear existing modifiers
            Game.Modifiers.Clear();

            // read file
            Game.Modifiers.AddRange(File.ReadAllLines(Environment.CurrentDirectory + ModifiersFileName));
        }

        /// <summary>
        /// Saves the ships.
        /// </summary>
        public static void SaveShips()
        {
            // if the directory doesn't exist, create it
            if (!Directory.Exists(Environment.CurrentDirectory + ShipDirectory))
                Directory.CreateDirectory(Environment.CurrentDirectory + ShipDirectory);

            foreach (var ship in Game.Ships)
                File.WriteAllText(Environment.CurrentDirectory + ShipDirectory + ship.Name + ".json", JsonConvert.SerializeObject(ship));

            Game.WriteLine($"Saved {Game.Ships.Count} ships", MessageType.Message);
        }
    }
}