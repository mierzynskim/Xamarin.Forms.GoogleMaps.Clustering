using System;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    public sealed class ClusterClickedEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
        public int ItemsCount { get; }

        internal ClusterClickedEventArgs(int itemsCount)
        {
            ItemsCount = itemsCount;
        }
    }
}