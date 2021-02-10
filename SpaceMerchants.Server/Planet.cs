//-----------------------------------------------------------------------
// <copyright file="Planet.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Planet
    /// </summary>
    public class Planet : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Planet"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="starSystem">The star system.</param>
        public Planet(string name, StarSystem starSystem)
            : base(name)
        {
            Contract.Requires(starSystem != null);

            StarSystem = starSystem;
            StarSystem.Planets.Add(this);
        }

        /// <summary>
        /// Gets the star system.
        /// </summary>
        /// <value>The star system.</value>
        public StarSystem StarSystem { get; }

        /// <summary>
        /// Gets the outposts.
        /// </summary>
        /// <value>The outposts.</value>
        public List<Outpost> Outposts { get; } = new List<Outpost>();

        /// <summary>
        /// Generates the specified minimum outposts.
        /// </summary>
        /// <param name="starSystem">The star system.</param>
        /// <param name="minOutposts">The minimum outposts.</param>
        /// <param name="maxOutposts">The maximum outposts.</param>
        /// <returns>The generated planet.</returns>
        public static Planet Generate(StarSystem starSystem, int minOutposts, int maxOutposts)
        {
            Contract.Requires(starSystem != null);
            Contract.Requires(minOutposts >= 0);
            Contract.Requires(maxOutposts >= 0);

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

                // ensure the name is unique in this star system
                if (!starSystem.Planets.Exists(p => p.Name == name))
                {
                    // create the new planet
                    var planet = new Planet(name, starSystem);

                    // choose outpost number
                    int outposts = Utility.RNG.Next(minOutposts, maxOutposts + 1);

                    // generate outposts and add them to the new planet
                    for (int i = 0; i < outposts; i++)
                        Outpost.Generate(planet);

                    return planet;
                }
            }
        }
    }
}