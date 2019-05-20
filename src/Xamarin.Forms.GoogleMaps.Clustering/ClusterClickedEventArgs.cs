using System;
using System.Collections.Generic;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    public sealed class ClusterClickedEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
        
        public int ItemsCount { get; }

        public IEnumerable<Pin> Pins { get; set; }

        internal ClusterClickedEventArgs(int itemsCount, IEnumerable<Pin> pins)
        {
            ItemsCount = itemsCount;
            Pins = pins;
        }
    }
}