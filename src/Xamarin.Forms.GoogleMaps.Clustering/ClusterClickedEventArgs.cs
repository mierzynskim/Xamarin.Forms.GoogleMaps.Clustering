using System;
using System.Collections.Generic;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    /// <summary>
    /// Defines the <see cref="ClusterClickedEventArgs" />
    /// </summary>
    public sealed class ClusterClickedEventArgs : EventArgs
    {
        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterClickedEventArgs"/> class.
        /// </summary>
        /// <param name="itemsCount">The itemsCount<see cref="int"/></param>
        /// <param name="pins">The pins<see cref="IEnumerable{Pin}"/></param>
        /// <param name="position">The position<see cref="Position"/></param>
        internal ClusterClickedEventArgs(int itemsCount, IEnumerable<Pin> pins, Position position)
        {
            ItemsCount = itemsCount;
            Pins = pins;
            Position = position;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ItemsCount
        /// </summary>
        public int ItemsCount { get; }

        /// <summary>
        /// Gets the Pins
        /// </summary>
        public IEnumerable<Pin> Pins { get; }

        /// <summary>
        /// Gets the Position
        /// </summary>
        public Position Position { get; }

        #endregion
    }
}
