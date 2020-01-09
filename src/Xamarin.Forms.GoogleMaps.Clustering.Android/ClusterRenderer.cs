using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V4.Content.Res;
using Android.Widget;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.View;
using Com.Google.Maps.Android.UI;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using Xamarin.Forms.Platform.Android;
using NativeBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    public class ClusterRenderer : DefaultClusterRenderer
    {
        private readonly ClusteredMap map;
        private readonly Context context;
        private readonly Dictionary<string, NativeBitmapDescriptor> disabledBucketsCache;
        private readonly Dictionary<string, NativeBitmapDescriptor> enabledBucketsCache;
        private IconGenerator iconGenerator;
        private ImageView markerView;

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

        private void AddToIconCache(ICluster cluster, NativeBitmapDescriptor icon)
        {
            var clusterText = GetClusterText(cluster);
            if (map.ClusterOptions.EnableBuckets)
                enabledBucketsCache.Add(clusterText, icon);
            else
                disabledBucketsCache.Add(clusterText, icon);
        }

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

        protected override int GetBucket(ICluster cluster)
        {
            var size = cluster.Size;
            if (size <= map.ClusterOptions.Buckets[0])
                return size;
            return map.ClusterOptions.Buckets[BucketIndexForSize(cluster.Size)];
        }

        protected override int GetColor(int size)
        {
            return map.ClusterOptions.BucketColors[BucketIndexForSize(size)].ToAndroid();
        }

        private string GetClusterText(ICluster cluster)
        {
            return GetClusterText(cluster.Size);
        }

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

        private int BucketIndexForSize(int size)
        {
            uint index = 0;
            var buckets = map.ClusterOptions.Buckets;

            while (index + 1 < buckets.Length && buckets[index + 1] <= size)
                ++index;

            return (int)index;
        }
    }
}