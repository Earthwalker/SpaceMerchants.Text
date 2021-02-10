//-----------------------------------------------------------------------
// <copyright file="Entity.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Entity
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Entity(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            Name = name;
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public string Name { get; set; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}