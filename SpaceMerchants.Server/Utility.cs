//-----------------------------------------------------------------------
// <copyright file="Utility.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets or sets the random number generator.
        /// </summary>
        /// <value>The random number generator.</value>
        public static Random RNG { get; set; } = new Random();

        /// <summary>
        /// Shuffles a list based off of the Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T">The list type.</typeparam>
        /// <param name="source">The list to shuffle.</param>
        public static void Shuffle<T>(this List<T> source)
        {
            Contract.Requires(source != null && source.Count > 0);

            if (source == null)
                return;

            int n = source.Count;
            while (n > 1)
            {
                n--;
                int k = RNG.Next(n + 1);
                T value = source[k];
                source[k] = source[n];
                source[n] = value;
            }
        }

        /// <summary>
        /// Picks a random item in the specified source.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>A random item in the list</returns>
        public static T Pick<T>(this List<T> source)
        {
            Contract.Requires(source != null && source.Count > 0);

            return source[RNG.Next(source.Count)];
        }

        /// <summary>
        /// Finds the next random <see cref="double"/> between two bounds limited to the specified digits.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value (inclusive).</param>
        /// <param name="digits">The digits.</param>
        /// <returns>The random value.</returns>
        public static double NextDouble(this Random source, double min, double max, int digits)
        {
            Contract.Requires(digits > 0);

            return source.Next((int)(min * Math.Pow(10, digits)), (int)(max * Math.Pow(10, digits) + 1)) / Math.Pow(10, digits);
        }

        /// <summary>
        /// Gets the starting price.
        /// </summary>
        /// <returns></returns>
        public static int StartingPrice()
        {
            return 100;
        }

        /// <summary>
        /// Gets the name of the month.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns>The name of the month.</returns>
        public static string GetMonthName(int month)
        {
            while (month < 1)
                month += 12;
            while (month > 12)
                month -= 12;

            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        /// <summary>
        /// Clamps the value to the specified minimum and maximum.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="source">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>The clamped value.</returns>
        public static T Clamp<T>(this T source, T min, T max)
            where T : IComparable<T>
        {
            if (source.CompareTo(min) < 0)
                return min;
            else if (source.CompareTo(max) > 0)
                return max;
            else
                return source;
        }

        /// <summary>
        /// Picks a random outpost based on size.
        /// </summary>
        /// <returns>The selected outpost.</returns>
        public static Outpost PickOutpost()
        {
            var planet = Game.StarSystems.Pick().Planets.Pick();
            var weightedOutpostNum = planet.Outposts.Sum(o => (int)o.Size);
            var randomIndex = RNG.Next(weightedOutpostNum + 1);
            int index = 0;

            foreach (var outpost in planet.Outposts)
            {
                index += (int)outpost.Size;

                if (randomIndex <= index)
                    return outpost;
            }

            return null;
        }

        /// <summary>
        /// Adds the range of values to the <see cref="ICollection{Type}"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="collection">The collection.</param>
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            foreach (var item in collection)
                source.Add(item);
        }
    }
}