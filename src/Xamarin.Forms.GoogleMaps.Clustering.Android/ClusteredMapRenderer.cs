using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps.Logics;
using Xamarin.Forms.GoogleMaps.Logics.Android;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    public class ClusteredMapRenderer : MapRenderer
    {
        readonly ClusterLogic clusterLogic;
        
        private ClusteredMap ClusteredMap => (ClusteredMap) Map;
        
        public ClusteredMapRenderer(Activity context) : base(context)
        {
            Logics.Remove(Logics.OfType<PinLogic>().First());
            clusterLogic = new ClusterLogic(context, Config.BitmapDescriptorFactory,
                OnClusteredMarkerCreating, OnClusteredMarkerCreated, OnClusteredMarkerDeleting,
                OnClusteredMarkerDeleted);
            Logics.Add(clusterLogic);
        }

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