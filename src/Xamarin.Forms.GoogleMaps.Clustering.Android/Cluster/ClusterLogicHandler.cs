using Com.Google.Maps.Android.Clustering;
using Xamarin.Forms.GoogleMaps.Logics.Android;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android.Cluster
{
    internal class ClusterLogicHandler : Java.Lang.Object,
        ClusterManager.IOnClusterClickListener,
        ClusterManager.IOnClusterItemClickListener,
        ClusterManager.IOnClusterInfoWindowClickListener,
        ClusterManager.IOnClusterItemInfoWindowClickListener
    {
        private Map map;
        private ClusterManager clusterManager;
        private ClusterLogic logic;

        public ClusterLogicHandler(Map map, ClusterManager manager, ClusterLogic logic)
        {
            this.map = map;
            clusterManager = manager;
            this.logic = logic;
        }

        public bool OnClusterClick(ICluster cluster)
        {
            //Toast.MakeText(XForms.Context, string.Format("{0} items in cluster", cluster.Items.Count), ToastLength.Short).Show();
            return false;
        }

        public bool OnClusterItemClick(Java.Lang.Object nativeItemObj)
        {
            var targetPin = logic.LookupPin(nativeItemObj as ClusteredMarker);
           
            targetPin?.SendTap();

            if (targetPin != null)
            {
                if (!ReferenceEquals(targetPin, map.SelectedPin))
                    map.SelectedPin = targetPin;
                map.SendPinClicked(targetPin);
            }

            return false;
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