using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System.Linq;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps.Logics.Android;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    /// <summary>
    /// Defines the <see cref="ClusteredMapRenderer" />
    /// </summary>
    public class ClusteredMapRenderer : MapRenderer
    {
        #region Variables

        /// <summary>
        /// Defines the clusterLogic
        /// </summary>
        readonly ClusterLogic clusterLogic;

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredMapRenderer"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Activity"/></param>
        public ClusteredMapRenderer(Activity context) : base(context)
        {
            Logics.Remove(Logics.OfType<PinLogic>().First());
            clusterLogic = new ClusterLogic(context, Config.BitmapDescriptorFactory,
                OnClusteredMarkerCreating, OnClusteredMarkerCreated, OnClusteredMarkerDeleting,
                OnClusteredMarkerDeleted);
            Logics.Add(clusterLogic);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ClusteredMap
        /// </summary>
        private ClusteredMap ClusteredMap => (ClusteredMap)Map;

        #endregion

        /// <summary>
        /// The OnMapReady
        /// </summary>
        /// <param name="nativeMap">The nativeMap<see cref="GoogleMap"/></param>
        /// <param name="map">The map<see cref="Map"/></param>
        protected override void OnMapReady(GoogleMap nativeMap, Map map)
        {
            base.OnMapReady(nativeMap, map);
            if (ClusteredMap.PendingClusterRequest)
                clusterLogic.HandleClusterRequest();
        }

        /// <summary>
        /// Call when before marker create.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">the marker options.</param>
        protected virtual void OnClusteredMarkerCreating(Pin outerItem, MarkerOptions innerItem)
        {
        }

        /// <summary>
        /// Call when after marker create.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">the clustered marker.</param>
        protected virtual void OnClusteredMarkerCreated(Pin outerItem, ClusteredMarker innerItem)
        {
        }

        /// <summary>
        /// Call when before marker delete.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">the clustered marker.</param>
        protected virtual void OnClusteredMarkerDeleting(Pin outerItem, ClusteredMarker innerItem)
        {
        }

        /// <summary>
        /// Call when after marker delete.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">the clustered marker.</param>
        protected virtual void OnClusteredMarkerDeleted(Pin outerItem, ClusteredMarker innerItem)
        {
        }
    }
}
