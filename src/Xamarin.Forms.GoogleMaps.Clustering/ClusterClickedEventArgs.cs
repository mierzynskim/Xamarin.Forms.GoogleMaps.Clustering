using System;
using System.Collections.Generic;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    public sealed class ClusterClickedEventArgs : EventArgs
    {
        public int ItemsCount { get; }

        public IEnumerable<Pin> Pins { get; }

        internal ClusterClickedEventArgs(int itemsCount, IEnumerable<Pin> pins)
        {
            ItemsCount = itemsCount;
            Pins = pins;
        }
    }
}