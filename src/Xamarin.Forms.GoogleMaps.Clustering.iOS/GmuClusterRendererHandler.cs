using CoreAnimation;
using CoreGraphics;
using CoreLocation;
using Foundation;
using GMCluster;
using Google.Maps;
using UIKit;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    internal class GmuClusterRendererHandler : GMUDefaultClusterRenderer
    {
        private readonly MapView nativeMap;
        private const double KGmuAnimationDuration = 0.5; 

        public GmuClusterRendererHandler(MapView mapView, IGMUClusterIconGenerator iconGenerator)
            : base(mapView, iconGenerator)
        {
            nativeMap = mapView;
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

            marker.ZIndex = 1;
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
            marker.Flat = clusteredMarker.Flat;
            return marker;
        }

        private static void AnimateMarker(CLLocationCoordinate2D position, Marker marker)
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = KGmuAnimationDuration;
            marker.Layer.Latitude = position.Latitude;
            marker.Layer.Longitude = position.Longitude;
            CATransaction.Commit();
        }
    }
}