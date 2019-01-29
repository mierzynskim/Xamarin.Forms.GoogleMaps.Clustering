using System.ComponentModel;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps.Logics.Android;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    public class ClusteredMapRenderer : MapRenderer
    {
        readonly ClusterLogic _clusterLogic;
        
        private ClusteredMap ClusteredMap => (ClusteredMap) Map;
        
        public ClusteredMapRenderer(Context context) : base(context)
        {
            _clusterLogic = new ClusterLogic(context, Config.BitmapDescriptorFactory,
                OnClusteredMarkerCreating, OnClusteredMarkerCreated, OnClusteredMarkerDeleting, OnClusteredMarkerDeleted);
        }

        protected internal override void UpdateMap(GoogleMap oldNativeMap, Map oldMap, GoogleMap nativeMap, Map map)
        {
            base.UpdateMap(oldNativeMap, oldMap, nativeMap, map);
            _clusterLogic.Register(oldNativeMap, oldMap, NativeMap, map);
            _clusterLogic.RestoreItems();
            _clusterLogic.OnMapPropertyChanged(new PropertyChangedEventArgs(Map.SelectedPinProperty.PropertyName));

            if (this.ClusteredMap.PendingClusterRequest)
            {
                this._clusterLogic.HandleClusterRequest();
            }
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