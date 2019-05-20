using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    public class ClusteredMap : Map
    {
        public static readonly BindableProperty ClusterOptionsProperty = BindableProperty.Create(nameof(ClusterOptionsProperty),
            typeof(ClusterOptions),
            typeof(ClusteredMap),
            default(ClusterOptions));

        public event EventHandler<ClusterClickedEventArgs> ClusterClicked;
        
        internal Action OnCluster { get; set; }

        internal bool PendingClusterRequest { get; set; }

        public ClusteredMap()
        {
            ClusterOptions = new ClusterOptions();
        }
        
        public ClusterOptions ClusterOptions
        {
            get => (ClusterOptions)GetValue(ClusterOptionsProperty);
            set => SetValue(ClusterOptionsProperty, value);
        }
        
        public void Cluster()
        {
            SendCluster();
        }
        
        private void SendCluster()
        {
            if (OnCluster != null)
                OnCluster.Invoke();
            else
                PendingClusterRequest = true;
        }
        
        internal bool SendClusterClicked(int itemsCount, IEnumerable<Pin> pins)
        {
            var args = new ClusterClickedEventArgs(itemsCount, pins);
            ClusterClicked?.Invoke(this, args);
            return args.Handled;
        }
    }
}