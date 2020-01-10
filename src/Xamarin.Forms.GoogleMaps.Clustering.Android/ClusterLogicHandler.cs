using Com.Google.Maps.Android.Clustering;
using System.Collections.Generic;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    /// <summary>
    /// Defines the <see cref="ClusterLogicHandler" />
    /// </summary>
    internal class ClusterLogicHandler : Java.Lang.Object,
        ClusterManager.IOnClusterClickListener,
        ClusterManager.IOnClusterItemClickListener,
        ClusterManager.IOnClusterInfoWindowClickListener,
        ClusterManager.IOnClusterItemInfoWindowClickListener
    {
        #region Variables

        /// <summary>
        /// Defines the map
        /// </summary>
        private ClusteredMap map;

        /// <summary>
        /// Defines the clusterManager
        /// </summary>
        private ClusterManager clusterManager;

        /// <summary>
        /// Defines the logic
        /// </summary>
        private ClusterLogic logic;

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterLogicHandler"/> class.
        /// </summary>
        /// <param name="map">The map<see cref="ClusteredMap"/></param>
        /// <param name="manager">The manager<see cref="ClusterManager"/></param>
        /// <param name="logic">The logic<see cref="ClusterLogic"/></param>
        public ClusterLogicHandler(ClusteredMap map, ClusterManager manager, ClusterLogic logic)
        {
            this.map = map;
            clusterManager = manager;
            this.logic = logic;
        }

        #endregion

        #region Public

        /// <summary>
        /// The OnClusterClick
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool OnClusterClick(ICluster cluster)
        {
            var pins = GetClusterPins(cluster);
            var clusterPosition = new Position(cluster.Position.Latitude, cluster.Position.Longitude);
            map.SendClusterClicked(cluster.Items.Count, pins, clusterPosition);
            return false;
        }

        /// <summary>
        /// The OnClusterItemClick
        /// </summary>
        /// <param name="nativeItemObj">The nativeItemObj<see cref="Java.Lang.Object"/></param>
        /// <returns>The <see cref="bool"/></returns>
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

        /// <summary>
        /// The OnClusterInfoWindowClick
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        public void OnClusterInfoWindowClick(ICluster cluster)
        {
        }

        /// <summary>
        /// The OnClusterItemInfoWindowClick
        /// </summary>
        /// <param name="nativeItemObj">The nativeItemObj<see cref="Java.Lang.Object"/></param>
        public void OnClusterItemInfoWindowClick(Java.Lang.Object nativeItemObj)
        {
            var targetPin = logic.LookupPin(nativeItemObj as ClusteredMarker);

            targetPin?.SendTap();

            if (targetPin != null)
            {
                map.SendInfoWindowClicked(targetPin);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// The GetClusterPins
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <returns>The <see cref="List{Pin}"/></returns>
        private List<Pin> GetClusterPins(ICluster cluster)
        {
            var pins = new List<Pin>();
            foreach (var item in cluster.Items)
            {
                var clusterItem = (ClusteredMarker)item;
                pins.Add(logic.LookupPin(clusterItem));
            }

            return pins;
        }

        #endregion
    }
}
