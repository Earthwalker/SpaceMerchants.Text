//-----------------------------------------------------------------------
// <copyright file="StarSystem.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// StarSystem
    /// </summary>
    public class StarSystem : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarSystem"/> class.
        /// </summary>
        public StarSystem(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the planets.
        /// </summary>
        /// <value>The planets.</value>
        public List<Planet> Planets { get; } = new List<Planet>();

        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <param name="minPlanets">The minimum planets.</param>
        /// <param name="maxPlanets">The maximum planets.</param>
        /// <param name="minPlanetOutposts">The minimum outposts per planet.</param>
        /// <param name="maxPlanetOutposts">The maximum outposts per planet.</param>
        /// <returns>The generated star system.</returns>
        public static StarSystem Generate(int minPlanets, int maxPlanets, int minPlanetOutposts, int maxPlanetOutposts)
        {
            Contract.Requires(minPlanets >= 0);
            Contract.Requires(maxPlanets >= 0);
            Contract.Requires(minPlanetOutposts >= 0);
            Contract.Requires(maxPlanetOutposts >= 0);

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

                // // ensure the name is unique in this galaxy
                if (!Game.StarSystems.Exists(s => s.Name == name))
                {
                    // create the new star system
                    var starSystem = new StarSystem(name);

                    // choose planet number
                    int planets = Utility.RNG.Next(minPlanets, maxPlanets + 1);

                    // generate planets and add them to the new star system
                    for (int p = 0; p < planets; p++)
                        Planet.Generate(starSystem, minPlanetOutposts, maxPlanetOutposts);

                    return starSystem;
                }
            }
        }
    }
}