using System;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using CoreLocation;
using Foundation;
using Google.Maps;
using Google.Maps.Utility;
using UIKit;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    internal class ClusterRendererHandler : DefaultClusterRenderer
    {
        private readonly MapView nativeMap;
        private const double AnimationDuration = 0.5; 
        private float maxClusterZoom = 20;
        private readonly nuint minimumClusterSize = 5;

        public ClusterRendererHandler(MapView mapView, ClusterIconGenerator iconGenerator)
            : base(mapView, iconGenerator)
        {
            nativeMap = mapView;
        }

        public Marker GetMarker(ClusteredMarker clusteredMarker) =>
            Markers?.FirstOrDefault(m => ReferenceEquals(m.UserData as ClusteredMarker, clusteredMarker));
        
        public void SetUpdateMarker(ClusteredMarker clusteredMarker)
        {
            var marker = GetMarker(clusteredMarker);
            if (marker == null) return;
            marker.Position = new CLLocationCoordinate2D(clusteredMarker.Position.Latitude, clusteredMarker.Position.Longitude);
            marker.Title = clusteredMarker.Title;
            marker.Snippet = clusteredMarker.Snippet;
            marker.Draggable = clusteredMarker.Draggable;
            marker.Rotation = clusteredMarker.Rotation;
            marker.GroundAnchor = clusteredMarker.GroundAnchor;
            marker.InfoWindowAnchor = clusteredMarker.InfoWindowAnchor;
            marker.Flat = clusteredMarker.Flat;
            marker.Opacity = clusteredMarker.Opacity;
            marker.Icon = clusteredMarker.Icon;
        }
        
        public override bool ShouldRenderAsCluster(ICluster cluster, float zoom)
        {
            return cluster.Count >= minimumClusterSize && zoom <= maxClusterZoom;
        }

        [Export("markerWithPosition:from:userData:clusterIcon:animated:")]
        public Marker MarkerWithPosition(CLLocationCoordinate2D position, CLLocationCoordinate2D from,
            NSObject userData, UIImage clusterIcon,
            bool animated)
        {
            var initialPosition = animated ? from : position;
            var marker = userData is ClusteredMarker ? 
                CreateSinglePin(userData, initialPosition) 
                : CreateGroup(clusterIcon, initialPosition);

            marker.Position = initialPosition;
            marker.UserData = userData;

            marker.Map = nativeMap;

            if (animated)
                AnimateMarker(position, marker);
            return marker;
        }

        private static Marker CreateGroup(UIImage clusterIcon, CLLocationCoordinate2D initialPosition)
        {
            var marker = Marker.FromPosition(initialPosition);
            marker.Icon = clusterIcon;
            marker.GroundAnchor = new CGPoint(0.5, 0.5);
            marker.ZIndex = 1;
            return marker;
        }

        private static Marker CreateSinglePin(NSObject userData, CLLocationCoordinate2D initialPosition)
        {
            var clusteredMarker = userData as ClusteredMarker;
            var marker = Marker.FromPosition(initialPosition);
            marker.Icon = clusteredMarker.Icon;
            marker.Title = clusteredMarker.Title;
            marker.Snippet = clusteredMarker.Snippet;
            marker.Draggable = clusteredMarker.Draggable;
            marker.Rotation = clusteredMarker.Rotation;
            marker.GroundAnchor = clusteredMarker.GroundAnchor;
            marker.InfoWindowAnchor = clusteredMarker.InfoWindowAnchor;
            marker.Flat = clusteredMarker.Flat;
            marker.Opacity = clusteredMarker.Opacity;
            marker.ZIndex = clusteredMarker.ZIndex;
            return marker;
        }

        private static void AnimateMarker(CLLocationCoordinate2D position, Marker marker)
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = AnimationDuration;
            marker.Layer.Latitude = position.Latitude;
            marker.Layer.Longitude = position.Longitude;
            CATransaction.Commit();
        }
    }
}