//-----------------------------------------------------------------------
// <copyright file="Direction.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GrayZone.Server
{
    /// <summary>
    /// Rounded directional movement for <see cref="Character"/> s.
    /// </summary>
    public static class Direction
    {
        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int East
        {
            get
            {
                return FromDegrees(0);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int Northeast
        {
            get
            {
                return FromDegrees(45);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int North
        {
            get
            {
                return FromDegrees(90);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int Northwest
        {
            get
            {
                return FromDegrees(135);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int West
        {
            get
            {
                return FromDegrees(180);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int Southwest
        {
            get
            {
                return FromDegrees(225);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int South
        {
            get
            {
                return FromDegrees(270);
            }
        }

        /// <summary>
        /// Gets the east direction value.
        /// </summary>
        /// <value>The east direction value.</value>
        public static int Southeast
        {
            get
            {
                return FromDegrees(315);
            }
        }

        /// <summary>
        /// Gets or sets the total number of possible directions.
        /// </summary>
        /// <value>The total number of possible directions.</value>
        public static int Count { get; set; } = 360;

        /// <summary>
        /// Calculates the direction from the specified degrees.
        /// </summary>
        /// <param name="degrees">The degrees.</param>
        /// <returns>The direction.</returns>
        public static int FromDegrees(int degrees)
        {
            // normalize degrees
            degrees = degrees % 360;

            if (degrees < 0)
                degrees += 360;

            return (int)(((float)degrees / 360) * Count);
        }
    }
}