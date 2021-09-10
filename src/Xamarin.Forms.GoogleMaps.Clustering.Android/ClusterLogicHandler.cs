using System.Collections.Generic;
using Com.Google.Maps.Android.Clustering;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    internal class ClusterLogicHandler : Java.Lang.Object,
        ClusterManager.IOnClusterClickListener,
        ClusterManager.IOnClusterItemClickListener,
        ClusterManager.IOnClusterInfoWindowClickListener,
        ClusterManager.IOnClusterItemInfoWindowClickListener
    {
        private ClusteredMap map;
        private ClusterManager clusterManager;
        private ClusterLogic logic;

        public ClusterLogicHandler(ClusteredMap map, ClusterManager manager, ClusterLogic logic)
        {
            this.map = map;
            clusterManager = manager;
            this.logic = logic;
        }

        public bool OnClusterClick(ICluster cluster)
        {
            var pins = GetClusterPins(cluster);
            var clusterPosition = new Position(cluster.Position.Latitude, cluster.Position.Longitude);
            return map.SendClusterClicked(cluster.Items.Count, pins, clusterPosition);            
        }

        private List<Pin> GetClusterPins(ICluster cluster)
        {
            var pins = new List<Pin>();
            foreach (var item in cluster.Items)
            {
                var clusterItem = (ClusteredMarker) item;
                pins.Add(logic.LookupPin(clusterItem));
            }

            return pins;
        }

        public bool OnClusterItemClick(Java.Lang.Object nativeItemObj)
        {
            var targetPin = logic.LookupPin(nativeItemObj as ClusteredMarker);
           
            var eventHandled = targetPin?.SendTap();

            if (targetPin != null)
            {
                if (!ReferenceEquals(targetPin, map.SelectedPin))
                    map.SelectedPin = targetPin;
                var pinClickedHandled = map.SendPinClicked(targetPin);
                if(eventHandled != true) //eigher not handled at all or user returned 'not handled' signal so this one will decide
                {
                    eventHandled = pinClickedHandled;
                }
            }

            return eventHandled ?? false; //either we handled it of return default 'false'
        }

        public void OnClusterInfoWindowClick(ICluster cluster)
        {

        }

        public void OnClusterItemInfoWindowClick(Java.Lang.Object nativeItemObj)
        {
            var targetPin = logic.LookupPin(nativeItemObj as ClusteredMarker);
           
            targetPin?.SendTap();

            if (targetPin != null)
            {
                map.SendInfoWindowClicked(targetPin);
            }
        }
    }
}