using CoreAnimation;
using CoreGraphics;
using CoreLocation;
using Foundation;
using Google.Maps;
using Google.Maps.Utility;
using System.Linq;
using UIKit;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    /// <summary>
    /// Defines the <see cref="ClusterRendererHandler" />
    /// </summary>
    internal class ClusterRendererHandler : DefaultClusterRenderer
    {
        #region Constants

        /// <summary>
        /// Defines the AnimationDuration
        /// </summary>
        private const double AnimationDuration = 0.5;

        #endregion

        #region Variables

        /// <summary>
        /// Defines the nativeMap
        /// </summary>
        private readonly MapView nativeMap;

        /// <summary>
        /// Defines the minimumClusterSize
        /// </summary>
        private readonly int minimumClusterSize;

        /// <summary>
        /// Defines the maxClusterZoom
        /// </summary>
        private float maxClusterZoom = 20;

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterRendererHandler"/> class.
        /// </summary>
        /// <param name="mapView">The mapView<see cref="MapView"/></param>
        /// <param name="iconGenerator">The iconGenerator<see cref="ClusterIconGenerator"/></param>
        /// <param name="minimumClusterSize">The minimumClusterSize<see cref="int"/></param>
        public ClusterRendererHandler(MapView mapView, ClusterIconGenerator iconGenerator, int minimumClusterSize)
            : base(mapView, iconGenerator)
        {
            nativeMap = mapView;
            this.minimumClusterSize = minimumClusterSize;
        }

        #endregion

        #region Public

        /// <summary>
        /// The GetMarker
        /// </summary>
        /// <param name="clusteredMarker">The clusteredMarker<see cref="ClusteredMarker"/></param>
        /// <returns>The <see cref="Marker"/></returns>
        public Marker GetMarker(ClusteredMarker clusteredMarker) =>
            Markers?.FirstOrDefault(m => ReferenceEquals(m.UserData as ClusteredMarker, clusteredMarker));

        /// <summary>
        /// The SetUpdateMarker
        /// </summary>
        /// <param name="clusteredMarker">The clusteredMarker<see cref="ClusteredMarker"/></param>
        public void SetUpdateMarker(ClusteredMarker clusteredMarker)
        {
            var marker = GetMarker(clusteredMarker);
            if (marker == null) return;
            marker.Position = new CLLocationCoordinate2D(clusteredMarker.Position.Latitude,
                clusteredMarker.Position.Longitude);
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

        /// <summary>
        /// The ShouldRenderAsCluster
        /// </summary>
        /// <param name="cluster">The cluster<see cref="ICluster"/></param>
        /// <param name="zoom">The zoom<see cref="float"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool ShouldRenderAsCluster(ICluster cluster, float zoom)
        {
            return cluster.Count > (uint)minimumClusterSize && zoom <= maxClusterZoom;
        }

        /// <summary>
        /// The MarkerWithPosition
        /// </summary>
        /// <param name="position">The position<see cref="CLLocationCoordinate2D"/></param>
        /// <param name="from">The from<see cref="CLLocationCoordinate2D"/></param>
        /// <param name="userData">The userData<see cref="NSObject"/></param>
        /// <param name="clusterIcon">The clusterIcon<see cref="UIImage"/></param>
        /// <param name="animated">The animated<see cref="bool"/></param>
        /// <returns>The <see cref="Marker"/></returns>
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

        #endregion

        #region Private

        /// <summary>
        /// The CreateGroup
        /// </summary>
        /// <param name="clusterIcon">The clusterIcon<see cref="UIImage"/></param>
        /// <param name="initialPosition">The initialPosition<see cref="CLLocationCoordinate2D"/></param>
        /// <returns>The <see cref="Marker"/></returns>
        private static Marker CreateGroup(UIImage clusterIcon, CLLocationCoordinate2D initialPosition)
        {
            var marker = Marker.FromPosition(initialPosition);
            marker.Icon = clusterIcon;
            marker.GroundAnchor = new CGPoint(0.5, 0.5);
            marker.ZIndex = 1;
            return marker;
        }

        /// <summary>
        /// The CreateSinglePin
        /// </summary>
        /// <param name="userData">The userData<see cref="NSObject"/></param>
        /// <param name="initialPosition">The initialPosition<see cref="CLLocationCoordinate2D"/></param>
        /// <returns>The <see cref="Marker"/></returns>
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

        /// <summary>
        /// The AnimateMarker
        /// </summary>
        /// <param name="position">The position<see cref="CLLocationCoordinate2D"/></param>
        /// <param name="marker">The marker<see cref="Marker"/></param>
        private static void AnimateMarker(CLLocationCoordinate2D position, Marker marker)
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = AnimationDuration;
            marker.Layer.Latitude = position.Latitude;
            marker.Layer.Longitude = position.Longitude;
            CATransaction.Commit();
        }

        #endregion
    }
}
