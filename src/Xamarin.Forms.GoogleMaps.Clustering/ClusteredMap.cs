using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    public class ClusteredMap : Map
    {
        public static readonly BindableProperty ClusterOptionsProperty = BindableProperty.Create(nameof(ClusterOptionsProperty),
            typeof(ClusterOptions),
            typeof(ClusteredMap),
            default(ClusterOptions));

        private readonly ObservableCollection<Pin> _clusteredPins = new ObservableCollection<Pin>();
        
        public event EventHandler<ClusterClickedEventArgs> ClusterClicked;
        
        internal Action OnCluster { get; set; }
        internal Action<Pin> OnMarkerUpdate { get; set; }

        internal bool PendingClusterRequest { get; set; }

        public ClusteredMap()
        {
            ClusterOptions = new ClusterOptions();
            _clusteredPins.CollectionChanged += (sender, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in args.OldItems)
                        item.PropertyChanged -= MarkerPropertyChanged;
                }
                
                if (args.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in args.NewItems)
                        item.PropertyChanged += MarkerPropertyChanged;
                }
            };
        }
        
        private void MarkerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != Pin.PositionProperty.PropertyName)
                OnMarkerUpdate?.Invoke((Pin) sender);
        }
        
        public ClusterOptions ClusterOptions
        {
            get => (ClusterOptions)GetValue(ClusterOptionsProperty);
            set => SetValue(ClusterOptionsProperty, value);
        }
        
        public IList<Pin> ClusteredPins => _clusteredPins;
        
        public void Cluster()
        {
            this.SendCluster();
        }
        
        private void SendCluster()
        {
            if (OnCluster != null)
            {
                OnCluster.Invoke();
            }
            else
            {
                PendingClusterRequest = true;
            }
        }
        
        internal bool SendClusterClicked(int itemsCount)
        {
            var args = new ClusterClickedEventArgs(itemsCount);
            ClusterClicked?.Invoke(this, args);
            return args.Handled;
        }
    }
}