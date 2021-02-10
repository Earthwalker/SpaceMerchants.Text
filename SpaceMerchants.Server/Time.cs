//-----------------------------------------------------------------------
// <copyright file="Time.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;

    /// <summary>
    /// Main game loop.
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// The timer used for ticking.
        /// </summary>
        private static Timer tickTimer;

        /// <summary>
        /// The backing variable for <see cref="TickPeriod"/>.
        /// </summary>
        private static int tickPeriod;

        /// <summary>
        /// The event handler for the <see cref="E:TickEvent"/> event.
        /// </summary>
        private static EventHandler<EventArgs> tickEventHandler;

        /// <summary>
        /// Occurs on each tick.
        /// </summary>
        public static event EventHandler<EventArgs> TickEvent
        {
            add
            {
                tickEventHandler += value;
            }

            remove
            {
                tickEventHandler -= value;
            }
        }

        /// <summary>
        /// Gets or sets the tick period in milliseconds.
        /// </summary>
        /// <value>The tick period.</value>
        public static int TickPeriod
        {
            get
            {
                return tickPeriod;
            }

            set
            {
                if (tickPeriod != value)
                {
                    tickPeriod = value;

                    if (tickTimer != null)
                        tickTimer.Change(0, tickPeriod);
                }
            }
        }

        /// <summary>
        /// Gets or sets the tick time span.
        /// </summary>
        /// <value>The tick time span.</value>
        public static TimeSpan TickTimeSpan { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets the date time.
        /// </summary>
        /// <value>The date time.</value>
        public static DateTime DateTime { get; private set; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public static void Start()
        {
            if (tickTimer == null)
                tickTimer = new Timer(OnTickEvent, null, 0, TickPeriod);
            else
                tickTimer.Change(0, TickPeriod);
        }

        /// <summary>
        /// Starts this instance with the specified tick period.
        /// </summary>
        /// <param name="tickPeriod">The tick period.</param>
        public static void Start(int tickPeriod)
        {
            Contract.Requires(tickPeriod > 0);

            Time.tickPeriod = tickPeriod;

            Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public static void Stop()
        {
            tickTimer?.Dispose();
            tickTimer = null;
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        public static void Pause()
        {
            tickTimer?.Change(Timeout.Infinite, TickPeriod);
        }

        /// <summary>
        /// Called on each tick.
        /// </summary>
        /// <param name="source">The source.</param>
        private static void OnTickEvent(object source)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            tickEventHandler(source, EventArgs.Empty);

            if (TickTimeSpan != TimeSpan.Zero)
                DateTime += TickTimeSpan;

            stopwatch.Stop();
            Game.WriteLine("Step time: " + stopwatch.Elapsed.ToString(), MessageType.Message);
        }
    }
}