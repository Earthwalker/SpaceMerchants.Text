//-----------------------------------------------------------------------
// <copyright file="Game.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
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
    /// Main game class.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The timer.
        /// </summary>
        private static Timer timer;

        /// <summary>
        /// The console colors.
        /// </summary>
        private static Dictionary<MessageType, ConsoleColor> colors = new Dictionary<MessageType, ConsoleColor>();

        /// <summary>
        /// The settings file name.
        /// </summary>
        public const string SettingsFileName = @"\settings.cfg";

        /// <summary>
        /// Gets or sets the host to connect to.
        /// </summary>
        /// <value>The host.</value>
        public static string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        /// <value>The port.</value>
        public static int Port { get; set; } = 11000;

        /// <summary>
        /// Gets or sets the connection key.
        /// </summary>
        /// <value>The host.</value>
        public static string ConnectionKey { get; set; }

        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        /// <value>The player.</value>
        public static Player Player { get; set; } = new Player();

        /// <summary>
        /// Gets or sets the networking client.
        /// </summary>
        /// <value>The networking client.</value>
        public static Client Client { get; set; }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public static void StartTimer()
        {
            if (timer == null)
                timer = new Timer(SendUpdate, null, 0, 1000);
        }

        /// <summary>
        /// Defines the program entry point.
        /// </summary>
        /// <param name="args">An array of <see cref="string"/> containing command line parameters.</param>
        private static void Main(string[] args)
        {
            // load settings
            LoadSettings();

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
            WriteLine("8.21.23", MessageType.Message);
            WriteLine();

            // listen for input while the game is running
            while (Player.MainMenuSelection != MainMenuItem.Exit)
            {
                Player.MainMenu();

                while (Player.MainMenuSelection != MainMenuItem.None && Player.MainMenuSelection != MainMenuItem.Exit)
                {
                    switch (Player.MainMenuSelection)
                    {
                        case MainMenuItem.Ship:
                            if (Player.ShipMenuSelection == ShipMenuItem.Name && !Player.ShipNameMenu())
                                continue;
                            else if (Player.ShipMenuSelection == ShipMenuItem.Class && !Player.SelectShipClassMenu())
                                continue;
                            else
                            {
                                WriteLine($"{Player.ShipClass} ship {Player.Name} ready", MessageType.Message);
                                Player.ShipMenuSelection = ShipMenuItem.Name;
                                Player.MainMenuSelection = MainMenuItem.None;
                            }
                            break;

                        case MainMenuItem.Join:
                            if (Client?.IsRunning != true)
                            {
                                if (Player.SelectHostMenu())
                                {
                                    Player.ReceivedReply = true;
                                    Player.PlayMenuSelection = PlayMenuItem.None;

                                    // create client and connect
                                    Client = new Client(Host, Port);

                                    StartTimer();

                                    // show help
                                    if (Client?.IsRunning == true)
                                        Player.ShowHelp();
                                }
                                else
                                    continue;
                            }
                            else
                            {
                                // exit to main menu if we get disconnected from the server
                                if (Client?.IsRunning == true)
                                {
                                    Player.PlayMenu();

                                    if (Player.PlayMenuSelection != PlayMenuItem.None)
                                        Player.ReadyToSend = true;

                                    // check if we are disconnecting
                                    if (Player.MainMenuSelection == MainMenuItem.None)
                                        Client.Stop();
                                }
                                else
                                    Player.MainMenuSelection = MainMenuItem.None;
                            }
                            break;
                    }
                }
            }

            SaveSettings();

            // shut everything down
            Client?.Stop();
            timer?.Dispose();
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private static void LoadSettings()
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
                        Player.Name = splitLine.Last().ToUpper();
                        break;

                    case "SHIPCLASS":
                        ShipClass shipClass;
                        if (Enum.TryParse(splitLine.Last().ToUpper(), true, out shipClass))
                            Player.ShipClass = shipClass;
                        break;

                    case "HOST":
                        Host = splitLine.Last();
                        break;

                    case "PORT":
                        ushort port;
                        if (ushort.TryParse(splitLine.Last(), out port))
                            Port = port;
                        break;

                    case "KEY":
                        ConnectionKey = splitLine.Last();
                        break;

                    case "DEFAULTCOLOR":
                        ConsoleColor defaultColor;

                        if (Enum.TryParse(splitLine.Last(), out defaultColor))
                            colors.Add(MessageType.Default, defaultColor);
                        break;

                    case "QUESTIONCOLOR":
                        ConsoleColor questionColor;

                        if (Enum.TryParse(splitLine.Last(), out questionColor))
                            colors.Add(MessageType.Question, questionColor);
                        break;

                    case "MESSAGECOLOR":
                        ConsoleColor messageColor;

                        if (Enum.TryParse(splitLine.Last(), out messageColor))
                            colors.Add(MessageType.Message, messageColor);
                        break;

                    case "ERRORCOLOR":
                        ConsoleColor errorColor;

                        if (Enum.TryParse(splitLine.Last(), out errorColor))
                            colors.Add(MessageType.Error, errorColor);
                        break;

                    case "INPUTCOLOR":
                        ConsoleColor inputColor;

                        if (Enum.TryParse(splitLine.Last(), out inputColor))
                            colors.Add(MessageType.Input, inputColor);
                        break;
                }
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private static void SaveSettings()
        {
            var lines = new List<string>();

            lines.Add($"Name={Player.Name}");
            lines.Add($"ShipClass={Player.ShipClass}");
            lines.Add($"Host={Host}");
            lines.Add($"Port={Port}");

            foreach (var color in colors)
            {
                switch (color.Key)
                {
                    case MessageType.Default:
                        lines.Add($"DefaultColor={color.Value}");
                        break;

                    case MessageType.Question:
                        lines.Add($"QuestionColor={color.Value}");
                        break;

                    case MessageType.Message:
                        lines.Add($"MessageColor={color.Value}");
                        break;

                    case MessageType.Error:
                        lines.Add($"ErrorColor={color.Value}");
                        break;

                    case MessageType.Input:
                        lines.Add($"InputColor={color.Value}");
                        break;
                }
            }

            File.WriteAllLines(Environment.CurrentDirectory + SettingsFileName, lines);
        }

        /// <summary>
        /// Sends an update.
        /// </summary>
        /// <param name="state">The state.</param>
        private static void SendUpdate(object state)
        {
            // check if we have input to send
            if (!Player.ReadyToSend || !Player.ReceivedReply)
                return;

            var writer = new NetDataWriter();

            // write player action
            writer.Put((byte)Player.PlayMenuSelection);

            // write player input
            writer.Put(Player.Input);

            // send the message
            Client.Send(writer);

            Player.Input = string.Empty;
            Player.ReadyToSend = false;

            if (Player.PlayMenuSelection != PlayMenuItem.None)
                Player.ReceivedReply = false;
        }

        /// <summary>
        /// Writes a line to the console.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="messageType">Type of the message.</param>
        public static void Write(string value, MessageType messageType = MessageType.Default)
        {
            if (colors.ContainsKey(messageType))
                Console.ForegroundColor = colors[messageType];
            else
                Console.ResetColor();

            Console.Write(value);

            if (colors.ContainsKey(MessageType.Input))
                Console.ForegroundColor = colors[MessageType.Input];
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
            if (colors.ContainsKey(messageType))
                Console.ForegroundColor = colors[messageType];
            else
                Console.ResetColor();

            Console.WriteLine(value);

            if (colors.ContainsKey(MessageType.Input))
                Console.ForegroundColor = colors[MessageType.Input];
            else
                Console.ResetColor();
        }

        /// <summary>
        /// Writes an empty line to the console.
        /// </summary>
        public static void WriteLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Determines whether the input is an exit string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Whether the input is an exit string.</returns>
        public static bool IsExitString(string input)
        {
            return input == "E" || input == "EXIT";
        }
    }
}