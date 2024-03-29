﻿//-----------------------------------------------------------------------
// <copyright file="Game.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using LiteNetLib.Utils;

    /// <summary>
    /// Types of console messages.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// The default.
        /// </summary>
        Default,

        /// <summary>
        /// The type for questions.
        /// </summary>
        Question,

        /// <summary>
        /// The type for messages.
        /// </summary>
        Message,

        /// <summary>
        /// The type for errors.
        /// </summary>
        Error,

        /// <summary>
        /// The type for input.
        /// </summary>
        Input
    }

    /// <summary>
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The amount of weight each level of <see cref="ShipClass"/> can carry.
        /// </summary>
        public const int MaxWeightMultiplier = 50;

        /// <summary>
        /// Gets or sets the maximum players.
        /// </summary>
        /// <value>The maximum players.</value>
        public static int MaxPlayers { get; set; }

        /// <summary>
        /// The console colors.
        /// </summary>
        public static Dictionary<MessageType, ConsoleColor> Colors { get; } = new Dictionary<MessageType, ConsoleColor>();

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public static int Port { get; set; }

        /// <summary>
        /// Gets or sets the connection key.
        /// </summary>
        /// <value>The port.</value>
        public static string ConnectionKey { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public static string Name { get; set; }

        /// <summary>
        /// Gets or sets the game hours per tick.
        /// </summary>
        /// <value>The game hours per tick.</value>
        public static int HoursPerTick { get; set; }

        /// <summary>
        /// Gets or sets the amount of time between ticks.
        /// </summary>
        /// <value>The amount of time between ticks.</value>
        public static int TickInterval { get; set; }

        /// <summary>
        /// Gets or sets the number of corporations to generate.
        /// </summary>
        /// <value>
        /// The number of corporations to generate.
        /// </value>
        public static int CorporationsToGenerate { get; set; }

        /// <summary>
        /// Gets or sets the number of star systems to generate.
        /// </summary>
        /// <value>
        /// The number of star systems to generate.
        /// </value>
        public static int StarSystemsToGenerate { get; set; }

        /// <summary>
        /// Gets or sets the number of ships to generate.
        /// </summary>
        /// <value>
        /// The number of ships to generate.
        /// </value>
        public static int ShipsToGenerate { get; set; }

        /// <summary>
        /// Gets or sets the minimum planets per system to generate.
        /// </summary>
        /// <value>
        /// The minimum planets per system to generate.
        /// </value>
        public static int MinPlanetsPerSystem { get; set; }

        /// <summary>
        /// Gets or sets the maximum planets per system to generate.
        /// </summary>
        /// <value>
        /// The maximum planets per system to generate.
        /// </value>
        public static int MaxPlanetsPerSystem { get; set; }

        /// <summary>
        /// Gets or sets the minimum outposts per planet to generate.
        /// </summary>
        /// <value>
        /// The minimum outposts per planet to generate.
        /// </value>
        public static int MinOutpostsPerPlanet { get; set; }

        /// <summary>
        /// Gets or sets the maximum outposts per planet to generate.
        /// </summary>
        /// <value>
        /// The maximum outposts per planet to generate.
        /// </value>
        public static int MaxOutpostsPerPlanet { get; set; }

        /// <summary>
        /// Gets the ships.
        /// </summary>
        /// <value>The ships.</value>
        public static List<Ship> Ships { get; } = new List<Ship>();

        /// <summary>
        /// Gets the item types.
        /// </summary>
        /// <value>The item types.</value>
        public static List<string> ItemTypes { get; } = new List<string>();

        /// <summary>
        /// Gets the headlines.
        /// </summary>
        /// <value>The headlines.</value>
        public static List<string> Headlines { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the star systems.
        /// </summary>
        /// <value>The star systems.</value>
        public static List<StarSystem> StarSystems { get; } = new List<StarSystem>();

        /// <summary>
        /// Gets the item names.
        /// </summary>
        /// <value>The item names.</value>
        public static List<string> ItemNames { get; } = new List<string>();

        /// <summary>
        /// Gets the ship names.
        /// </summary>
        /// <value>The ship names.</value>
        public static List<string> ShipNames { get; } = new List<string>();

        /// <summary>
        /// Gets the location names.
        /// </summary>
        /// <value>The location names.</value>
        public static List<string> LocationNames { get; } = new List<string>();

        /// <summary>
        /// Gets the modifiers.
        /// </summary>
        /// <value>The modifiers.</value>
        public static List<string> Modifiers { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the networking server.
        /// </summary>
        /// <value>The networking client.</value>
        public static Server Server { get; set; }

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            // load settings
            FileIO.LoadAll();

            // initiate tracer to log to log.txt
            InitiateTracer();

            // display fancy welcome message
            WriteLine(@"   _____
  / ____|
 | (___  _ __   __ _  ___ ___
  \___ \| '_ \ / _` |/ __/ _ \
  ____) | |_) | (_| | (_|  __/
 |_____/| .__/ \__,_|\___\___|           _
 |  \/  | |           | |               | |
 | \  / |_|__ _ __ ___| |__   __ _ _ __ | |_ ___
 | |\/| |/ _ \ '__/ __| '_ \ / _` | '_ \| __/ __|
 | |  | |  __/ | | (__| | | | (_| | | | | |_\__ \
 |_|  |_|\___|_|  \___|_| |_|\__,_|_| |_|\__|___/
                ", MessageType.Message);

            WriteLine("A game by Leamware", MessageType.Message);
            WriteLine("10.7.23", MessageType.Message);
            WriteLine();

            // generate galaxy
            Generate();

            // create server
            Server = new Server(Port)
            {
                MaxPlayers = MaxPlayers,
                ConnectionKey = ConnectionKey
            };

            // set tick time span
            Time.TickEvent += SendUpdate;
            Time.TickTimeSpan = new TimeSpan(HoursPerTick, 0, 0);
            Time.Start(TickInterval);

            ServerAdmin.ShowHelp();

            // listen for input while the game is running
            while (Server.IsRunning)
                ServerAdmin.MainMenu();

            // shut everything down
            Server.Stop();
        }

        /// <summary>
        /// Generates the galaxy.
        /// </summary>
        public static void Generate()
        {
            // generate star systems
            StarSystems.Clear();
            for (int i = 0; i < StarSystemsToGenerate; i++)
                StarSystems.Add(StarSystem.Generate(MinPlanetsPerSystem, MaxPlanetsPerSystem, MinOutpostsPerPlanet, MaxOutpostsPerPlanet));

            // generate ships
            Ships.Clear();
            for (int i = 0; i < ShipsToGenerate; i++)
                Ships.Add(Ship.Generate());
        }

        /// <summary>
        /// Generates an item.
        /// </summary>
        /// <param name="itemType">Type of the item to generate.</param>
        /// <returns>The generated item.</returns>
        public static string GenerateItem(string itemType = "")
        {
            // pick an item type if one is not provided
            if (string.IsNullOrEmpty(itemType))
                itemType = ItemTypes.Pick();

            string[] splitName;
            string[] splitModifier;

            while (true)
            {
                // pick a name
                splitName = ItemNames.Pick().Split('.');

                // ensure this is of the specified type
                if (splitName.First() == itemType)
                {
                    // pick a modifier
                    splitModifier = Modifiers.Pick().Split('.');

                    // add modifier if it matches and return the new item
                    if (splitModifier.First() == splitName.First())
                        return $"{splitName.First()}.{splitModifier.Last()} {splitName.Last()}";

                    // return the new item
                    return $"{splitName.First()}.{splitName.Last()}";
                }
            }
        }

        /// <summary>
        /// Sends a update.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public static void SendUpdate(object sender, EventArgs e)
        {
            foreach (var player in Server.Players)
            {
                if (player.Value == null)
                    continue;

                var writer = new NetDataWriter();
                bool send = false;

                // send batch update for all the players on a connection
                int lines = player.Value.Replies.Count;

                if (lines > 0)
                {
                    writer.Put((byte)lines);

                    foreach (var reply in player.Value.Replies)
                    {
                        writer.Put(reply.Key);
                        writer.Put((byte)reply.Value);
                    }

                    player.Value.OldReplies.Clear();
                    player.Value.OldReplies.AddRange(player.Value.Replies);
                    player.Value.Replies.Clear();

                    send = true;
                }

                // send if we have something to send
                if (send)
                    Server.Send(player.Key, writer);
            }

            // show stats at midnight
            //if (Time.DateTime.Hour == 0)
            //    ServerAdmin.SelectMainMenuItem(MainMenuItem.Data);
        }

        /// <summary>
        /// Finds the player with thee specified <see cref="Ship"/>.
        /// </summary>
        /// <param name="ship">The ship.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static void SendMessage(Ship ship, string message, MessageType messageType = MessageType.Default)
        {
            Contract.Requires(ship != null);
            Contract.Requires(!string.IsNullOrEmpty(message));

            foreach (var player in Server.Players)
            {
                if (player.Value == null)
                    continue;

                if (player.Value.Ship == ship)
                    player.Value.Replies.Add(message, messageType);
            }
        }

        /// <summary>
        /// Writes a line to the console.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="messageType">Type of the message.</param>
        public static void Write(string value, MessageType messageType = MessageType.Default)
        {
            Contract.Requires(!string.IsNullOrEmpty(value));

            if (Colors.ContainsKey(messageType))
                Console.ForegroundColor = Colors[messageType];
            else
                Console.ResetColor();

            Trace.Write(value);

            if (Colors.ContainsKey(MessageType.Input))
                Console.ForegroundColor = Colors[MessageType.Input];
            else
                Console.ResetColor();
        }

        /// <summary>
        /// Writes a line to the console.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="messageType">Type of the message.</param>
        public static void WriteLine(string value, MessageType messageType = MessageType.Default)
        {
            Contract.Requires(!string.IsNullOrEmpty(value));

            if (Colors.ContainsKey(messageType))
                Console.ForegroundColor = Colors[messageType];
            else
                Console.ResetColor();

            Trace.WriteLine(value);

            if (Colors.ContainsKey(MessageType.Input))
                Console.ForegroundColor = Colors[MessageType.Input];
            else
                Console.ResetColor();
        }

        /// <summary>
        /// Writes an empty line to the console.
        /// </summary>
        public static void WriteLine()
        {
            Trace.WriteLine(Environment.NewLine);
        }

        /// <summary>  
        /// Initiates a Tracer which will print to both  
        /// the Console and to a log file, log.txt  
        /// </summary>  
        private static void InitiateTracer()
        {
            Trace.Listeners.Clear();
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var twtl = new TextWriterTraceListener("log.txt")
            {
                Name = "TextLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };
            var ctl = new ConsoleTraceListener(false) { TraceOutputOptions = TraceOptions.DateTime };
            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }
    }
}