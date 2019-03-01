using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Util;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.View;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using Xamarin.Forms.Platform.Android;
using NativeBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android.Cluster
{
    public class ClusterRenderer : DefaultClusterRenderer
    {
        private readonly ClusteredMap map;
        private readonly SparseArray<NativeBitmapDescriptor> standardCache;
        private readonly SparseArray<NativeBitmapDescriptor> icons;
        private readonly float density;

        public ClusterRenderer(Context context,
            ClusteredMap map,
            GoogleMap nativeMap,
            ClusterManager manager)
            : base(context, nativeMap, manager)
        {
            this.map = map;
            standardCache = new SparseArray<NativeBitmapDescriptor>();
            icons = new SparseArray<NativeBitmapDescriptor>();
        }

        public void SetUpdateMarker(ClusteredMarker clusteredMarker)
        {
            var marker = GetMarker(clusteredMarker);
            marker.Position = new LatLng(clusteredMarker.Position.Latitude, clusteredMarker.Position.Longitude);
            marker.Title = clusteredMarker.Title;
            marker.Snippet = clusteredMarker.Snippet;
            marker.Draggable = clusteredMarker.Draggable;
            marker.Rotation = clusteredMarker.Rotation;
            marker.SetAnchor(clusteredMarker.AnchorX, clusteredMarker.AnchorY);
            marker.SetInfoWindowAnchor(clusteredMarker.InfoWindowAnchorX, clusteredMarker.InfoWindowAnchorY);
            marker.Flat = clusteredMarker.Flat;
            marker.Alpha = 1f - clusteredMarker.Alpha;
            marker.SetIcon(clusteredMarker.Icon);
        }

        protected override void OnBeforeClusterRendered(ICluster cluster, MarkerOptions options)
        {
            if (map.ClusterOptions.RendererCallback != null)
            {
                if (map.ClusterOptions.EnableBuckets)
                {
                    var bucketIndex = BucketIndexForSize(cluster.Size);
                    var icon = icons.Get(bucketIndex);
                    if (icon == null)
                    {
                        icon = DefaultBitmapDescriptorFactory.Instance.ToNative(map.ClusterOptions.RendererCallback(GetClusterText(cluster)));
                        icons.Put(bucketIndex, icon);
                    }

                    options.SetIcon(icon);
                }
                else
                {
                    var bucketIndex = BucketIndexForSize(cluster.Size);
                    var icon = standardCache.Get(bucketIndex);
                    if (icon == null)
                    {
                        icon = DefaultBitmapDescriptorFactory.Instance.ToNative(map.ClusterOptions.RendererCallback(cluster.Size.ToString()));
                        standardCache.Put(bucketIndex, icon);
                    }

                    options.SetIcon(icon);
                }
            }
            else if (map.ClusterOptions.RendererImage != null)
            {
                if (map.ClusterOptions.EnableBuckets)
                {
                    var bucketIndex = BucketIndexForSize(cluster.Size);
                    var icon = icons.Get(bucketIndex);
                    if (icon == null)
                    {
                        icon = DefaultBitmapDescriptorFactory.Instance.ToNative(map.ClusterOptions.RendererImage);
                        icons.Put(bucketIndex, icon);
                    }

                    options.SetIcon(icon);
                }
                else
                {
                    var bucketIndex = BucketIndexForSize(cluster.Size);
                    var icon = standardCache.Get(bucketIndex);
                    if (icon == null)
                    {
                        icon = DefaultBitmapDescriptorFactory.Instance.ToNative(map.ClusterOptions.RendererImage);
                        standardCache.Put(bucketIndex, icon);
                    }

                    options.SetIcon(icon);
                }
            }
            else
                base.OnBeforeClusterRendered(cluster, options);
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
            string result;
            var size = cluster.Size;

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
            {
                ++index;
            }

            return (int)index;
        }
    }
}