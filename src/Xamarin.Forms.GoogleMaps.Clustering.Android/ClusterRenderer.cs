using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.View;
using Com.Google.Maps.Android.UI;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using Xamarin.Forms.Platform.Android;
using NativeBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    /// <summary>
    /// Defines the <see cref="ClusterRenderer" />
    /// </summary>
    public class ClusterRenderer : DefaultClusterRenderer
    {
        #region Variables

        /// <summary>
        /// Defines the map
        /// </summary>
        private readonly ClusteredMap map;

        /// <summary>
        /// Defines the context
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// Defines the disabledBucketsCache
        /// </summary>
        private readonly Dictionary<string, NativeBitmapDescriptor> disabledBucketsCache;

        /// <summary>
        /// Defines the enabledBucketsCache
        /// </summary>
        private readonly Dictionary<string, NativeBitmapDescriptor> enabledBucketsCache;

        /// <summary>
        /// Defines the iconGenerator
        /// </summary>
        private IconGenerator iconGenerator;

        /// <summary>
        /// Defines the markerView
        /// </summary>
        private ImageView markerView;

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterRenderer"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Activity"/></param>
        /// <param name="map">The map<see cref="ClusteredMap"/></param>
        /// <param name="nativeMap">The nativeMap<see cref="GoogleMap"/></param>
        /// <param name="manager">The manager<see cref="ClusterManager"/></param>
        public ClusterRenderer(Activity context,
            ClusteredMap map,
            GoogleMap nativeMap,
            ClusterManager manager)
            : base(context, nativeMap, manager)
        {
            this.map = map;
            this.context = context;
            MinClusterSize = map.ClusterOptions.MinimumClusterSize;
            disabledBucketsCache = new Dictionary<string, NativeBitmapDescriptor>();
            enabledBucketsCache = new Dictionary<string, NativeBitmapDescriptor>();

            //Retrieve views from AXML to display groups of markers (clustering)
            var viewMarkerClusterGrouped = context.LayoutInflater.Inflate(Resource.Layout.marker_cluster_grouped, null);
            markerView = viewMarkerClusterGrouped.FindViewById<ImageView>(Resource.Id.marker_cluster_grouped_imageview);

            //Configure the groups of markers icon generator with the view. The icon generator will be used to display the marker's picture with a text
            iconGenerator = new IconGenerator(context);
            iconGenerator.SetContentView(viewMarkerClusterGrouped);
            iconGenerator.SetBackground(null);
        }

        #endregion

        #region Public

        /// <summary>
        /// The SetUpdateMarker
        /// </summary>
        /// <param name="clusteredMarker">The clusteredMarker<see cref="ClusteredMarker"/></param>
        public void SetUpdateMarker(ClusteredMarker clusteredMarker)
        {
            var marker = GetMarker(clusteredMarker);
            if (marker == null) return;
            marker.Position = new LatLng(clusteredMarker.Position.Latitude, clusteredMarker.Position.Longitude);
            marker.Title = clusteredMarker.Title;
            marker.Snippet = clusteredMarker.Snippet;
            marker.Draggable = clusteredMarker.Draggable;
            marker.Rotation = clusteredMarker.Rotation;
            marker.SetAnchor(clusteredMarker.AnchorX, clusteredMarker.AnchorY);
            marker.SetInfoWindowAnchor(clusteredMarker.InfoWindowAnchorX, clusteredMarker.InfoWindowAnchorY);
            marker.Flat = clusteredMarker.Flat;
            marker.Alpha = clusteredMarker.Alpha;
            marker.SetIcon(clusteredMarker.Icon);
        }

        #endregion

        #region Private

        /// <summary>
        /// The GetIcon
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <param name="descriptor">The descriptor<see cref="BitmapDescriptor"/></param>
        /// <returns>The <see cref="NativeBitmapDescriptor"/></returns>
        private NativeBitmapDescriptor GetIcon(ICluster cluster, BitmapDescriptor descriptor)
        {
            var bitmapDescriptorFactory = DefaultBitmapDescriptorFactory.Instance;
            var icon = GetFromIconCache(cluster);
            if (icon == null)
            {
                icon = bitmapDescriptorFactory.ToNative(descriptor);
                AddToIconCache(cluster, icon);
            }
            return icon;
        }

        /// <summary>
        /// The GetFromIconCache
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <returns>The <see cref="NativeBitmapDescriptor"/></returns>
        private NativeBitmapDescriptor GetFromIconCache(ICluster cluster)
        {
            NativeBitmapDescriptor bitmapDescriptor;
            var clusterText = GetClusterText(cluster);
            if (map.ClusterOptions.EnableBuckets)
                enabledBucketsCache.TryGetValue(clusterText, out bitmapDescriptor);
            else
                disabledBucketsCache.TryGetValue(clusterText, out bitmapDescriptor);
            return bitmapDescriptor;
        }

        /// <summary>
        /// The AddToIconCache
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <param name="icon">The icon<see cref="NativeBitmapDescriptor"/></param>
        private void AddToIconCache(ICluster cluster, NativeBitmapDescriptor icon)
        {
            var clusterText = GetClusterText(cluster);
            if (map.ClusterOptions.EnableBuckets)
                enabledBucketsCache.Add(clusterText, icon);
            else
                disabledBucketsCache.Add(clusterText, icon);
        }

        /// <summary>
        /// The GetClusterText
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string GetClusterText(ICluster cluster)
        {
            return GetClusterText(cluster.Size);
        }

        /// <summary>
        /// The BucketIndexForSize
        /// </summary>
        /// <param name="size">The size<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        private int BucketIndexForSize(int size)
        {
            uint index = 0;
            var buckets = map.ClusterOptions.Buckets;

            while (index + 1 < buckets.Length && buckets[index + 1] <= size)
                ++index;

            return (int)index;
        }

        #endregion

        /// <summary>
        /// The OnBeforeClusterRendered
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <param name="options">The options<see cref="MarkerOptions"/></param>
        protected override void OnBeforeClusterRendered(ICluster cluster, MarkerOptions options)
        {
            NativeBitmapDescriptor icon;
            if (map.ClusterOptions.RendererCallback != null)
            {
                var descriptorFromCallback =
                    map.ClusterOptions.RendererCallback(map.ClusterOptions.EnableBuckets ?
                        GetClusterText(cluster) : cluster.Size.ToString());
                icon = GetIcon(cluster, descriptorFromCallback);
                options.SetIcon(icon);
            }
            else if (map.ClusterOptions.RendererImage != null)
            {
                icon = GetIcon(cluster, map.ClusterOptions.RendererImage);
                options.SetIcon(icon);
            }
            else if (!map.ClusterOptions.EnableBuckets)
            {
                markerView.SetBackgroundResource(Resource.Drawable.marker_cluster);
                var shape = (GradientDrawable)markerView.Background;
                shape.SetColorFilter(new PorterDuffColorFilter(map.ClusterOptions.BucketColors[BucketIndexForSize(cluster.Size)].ToAndroid(), PorterDuff.Mode.SrcAtop));
                Bitmap iconBitmap = iconGenerator.MakeIcon(cluster.Size.ToString());
                options.SetIcon(global::Android.Gms.Maps.Model.BitmapDescriptorFactory.FromBitmap(iconBitmap));
            }
            else
                base.OnBeforeClusterRendered(cluster, options);
        }

        /// <summary>
        /// The OnBeforeClusterItemRendered
        /// </summary>
        /// <param name="marker">The marker<see cref="Java.Lang.Object"/></param>
        /// <param name="options">The options<see cref="MarkerOptions"/></param>
        protected override void OnBeforeClusterItemRendered(Java.Lang.Object marker, MarkerOptions options)
        {
            var clusteredMarker = marker as ClusteredMarker;

            options.SetTitle(clusteredMarker.Title)
                .SetSnippet(clusteredMarker.Snippet)
                .SetSnippet(clusteredMarker.Snippet)
                .Draggable(clusteredMarker.Draggable)
                .SetRotation(clusteredMarker.Rotation)
                .Anchor(clusteredMarker.AnchorX, clusteredMarker.AnchorY)
                .InfoWindowAnchor(clusteredMarker.InfoWindowAnchorX, clusteredMarker.InfoWindowAnchorY)
                .SetAlpha(clusteredMarker.Alpha)
                .Flat(clusteredMarker.Flat);

            if (clusteredMarker.Icon != null)
                options.SetIcon(clusteredMarker.Icon);

            base.OnBeforeClusterItemRendered(marker, options);
        }

        /// <summary>
        /// The GetBucket
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <returns>The <see cref="int"/></returns>
        protected override int GetBucket(ICluster cluster)
        {
            var size = cluster.Size;
            if (size <= map.ClusterOptions.Buckets[0])
                return size;
            return map.ClusterOptions.Buckets[BucketIndexForSize(cluster.Size)];
        }

        /// <summary>
        /// The GetColor
        /// </summary>
        /// <param name="size">The size<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        protected override int GetColor(int size)
        {
            return map.ClusterOptions.BucketColors[BucketIndexForSize(size)].ToAndroid();
        }

        /// <summary>
        /// The GetClusterText
        /// </summary>
        /// <param name="size">The size<see cref="int"/></param>
        /// <returns>The <see cref="string"/></returns>
        protected override string GetClusterText(int size)
        {
            string result;
            if (map.ClusterOptions.EnableBuckets)
            {
                var buckets = map.ClusterOptions.Buckets;
                var bucketIndex = BucketIndexForSize(size);

                result = size < buckets[0] ? size.ToString() : $"{buckets[bucketIndex]}+";
            }
            else
                result = size.ToString();

            return result;
        }
    }
}
