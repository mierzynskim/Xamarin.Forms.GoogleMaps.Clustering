using CoreGraphics;
using Foundation;
using Google.Maps;
using Google.Maps.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms.GoogleMaps.iOS.Extensions;
using Xamarin.Forms.GoogleMaps.iOS.Factories;
using Xamarin.Forms.GoogleMaps.Logics;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    /// <summary>
    /// Defines the <see cref="ClusterLogic" />
    /// </summary>
    internal class ClusterLogic : DefaultPinLogic<ClusteredMarker, MapView>
    {
        #region Variables

        /// <summary>
        /// Defines the clusterManager
        /// </summary>
        private ClusterManager clusterManager;

        /// <summary>
        /// Defines the onMarkerEvent
        /// </summary>
        private bool onMarkerEvent;

        /// <summary>
        /// Defines the draggingPin
        /// </summary>
        private Pin draggingPin;

        /// <summary>
        /// Defines the withoutUpdateNative
        /// </summary>
        private volatile bool withoutUpdateNative;

        /// <summary>
        /// Defines the onMarkerCreating
        /// </summary>
        private readonly Action<Pin, Marker> onMarkerCreating;

        /// <summary>
        /// Defines the onMarkerCreated
        /// </summary>
        private readonly Action<Pin, Marker> onMarkerCreated;

        /// <summary>
        /// Defines the onMarkerDeleting
        /// </summary>
        private readonly Action<Pin, Marker> onMarkerDeleting;

        /// <summary>
        /// Defines the onMarkerDeleted
        /// </summary>
        private readonly Action<Pin, Marker> onMarkerDeleted;

        /// <summary>
        /// Defines the imageFactory
        /// </summary>
        private readonly IImageFactory imageFactory;

        /// <summary>
        /// Defines the clusterRenderer
        /// </summary>
        private ClusterRendererHandler clusterRenderer;

        /// <summary>
        /// Defines the itemsDictionary
        /// </summary>
        private readonly Dictionary<NSObject, Pin> itemsDictionary = new Dictionary<NSObject, Pin>();

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterLogic"/> class.
        /// </summary>
        /// <param name="imageFactory">The imageFactory<see cref="IImageFactory"/></param>
        /// <param name="onMarkerCreating">The onMarkerCreating<see cref="Action{Pin, Marker}"/></param>
        /// <param name="onMarkerCreated">The onMarkerCreated<see cref="Action{Pin, Marker}"/></param>
        /// <param name="onMarkerDeleting">The onMarkerDeleting<see cref="Action{Pin, Marker}"/></param>
        /// <param name="onMarkerDeleted">The onMarkerDeleted<see cref="Action{Pin, Marker}"/></param>
        public ClusterLogic(
            IImageFactory imageFactory,
            Action<Pin, Marker> onMarkerCreating,
            Action<Pin, Marker> onMarkerCreated,
            Action<Pin, Marker> onMarkerDeleting,
            Action<Pin, Marker> onMarkerDeleted)
        {
            this.imageFactory = imageFactory;
            this.onMarkerCreating = onMarkerCreating;
            this.onMarkerCreated = onMarkerCreated;
            this.onMarkerDeleting = onMarkerDeleting;
            this.onMarkerDeleted = onMarkerDeleted;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ClusteredMap
        /// </summary>
        private ClusteredMap ClusteredMap => (ClusteredMap)Map;

        #endregion

        #region Private

        /// <summary>
        /// The GetClusterAlgorithm
        /// </summary>
        /// <param name="clusteredNewMap">The clusteredNewMap<see cref="ClusteredMap"/></param>
        /// <returns>The <see cref="IClusterAlgorithm"/></returns>
        private static IClusterAlgorithm GetClusterAlgorithm(ClusteredMap clusteredNewMap)
        {
            IClusterAlgorithm algorithm;
            switch (clusteredNewMap.ClusterOptions.Algorithm)
            {
                case ClusterAlgorithm.GridBased:
                    algorithm = new GridBasedClusterAlgorithm();
                    break;
                case ClusterAlgorithm.VisibleNonHierarchicalDistanceBased:
                    throw new NotSupportedException("VisibleNonHierarchicalDistanceBased is only supported on Android");
                default:
                    algorithm = new NonHierarchicalDistanceBasedAlgorithm();
                    break;
            }

            return algorithm;
        }

        /// <summary>
        /// The UpdateSelectedPin
        /// </summary>
        /// <param name="pin">The pin<see cref="Pin"/></param>
        private void UpdateSelectedPin(Pin pin)
        {
            if (pin != null)
                NativeMap.SelectedMarker = (ClusteredMarker)pin.NativeObject;
            else
                NativeMap.SelectedMarker = null;
        }

        /// <summary>
        /// The LookupPin
        /// </summary>
        /// <param name="marker">The marker<see cref="Marker"/></param>
        /// <returns>The <see cref="Pin"/></returns>
        private Pin LookupPin(Marker marker)
        {
            var associatedClusteredMarker = marker.UserData;
            return associatedClusteredMarker != null ? itemsDictionary[associatedClusteredMarker] : null;
        }

        /// <summary>
        /// The HandleClusterRequest
        /// </summary>
        private void HandleClusterRequest()
        {
            clusterManager.Cluster();
        }

        /// <summary>
        /// The OnInfoTapped
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GMSMarkerEventEventArgs"/></param>
        private void OnInfoTapped(object sender, GMSMarkerEventEventArgs e)
        {
            var targetPin = LookupPin(e.Marker);

            targetPin?.SendTap();

            if (targetPin != null)
            {
                Map.SendInfoWindowClicked(targetPin);
            }
        }

        /// <summary>
        /// The OnInfoLongPressed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GMSMarkerEventEventArgs"/></param>
        private void OnInfoLongPressed(object sender, GMSMarkerEventEventArgs e)
        {
            var targetPin = LookupPin(e.Marker);

            if (targetPin != null)
                Map.SendInfoWindowLongClicked(targetPin);
        }

        /// <summary>
        /// The HandleGmsTappedMarker
        /// </summary>
        /// <param name="mapView">The mapView<see cref="MapView"/></param>
        /// <param name="marker">The marker<see cref="Marker"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool HandleGmsTappedMarker(MapView mapView, Marker marker)
        {
            if (marker?.UserData is ICluster cluster)
            {
                var pins = GetClusterPins(cluster);
                var clusterPosition = new Position(cluster.Position.Latitude, cluster.Position.Longitude);
                ClusteredMap.SendClusterClicked((int)cluster.Count, pins, clusterPosition);
                return true;
            }
            var targetPin = LookupPin(marker);

            if (Map.SendPinClicked(targetPin))
                return true;

            try
            {
                onMarkerEvent = true;
                if (targetPin != null && !ReferenceEquals(targetPin, Map.SelectedPin))
                    Map.SelectedPin = targetPin;
            }
            finally
            {
                onMarkerEvent = false;
            }

            return false;
        }

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
                pins.Add(itemsDictionary[clusterItem]);
            }

            return pins;
        }

        /// <summary>
        /// The InfoWindowClosed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GMSMarkerEventEventArgs"/></param>
        private void InfoWindowClosed(object sender, GMSMarkerEventEventArgs e)
        {
            var targetPin = LookupPin(e.Marker);

            try
            {
                onMarkerEvent = true;
                if (targetPin != null && ReferenceEquals(targetPin, Map.SelectedPin))
                    Map.SelectedPin = null;
            }
            finally
            {
                onMarkerEvent = false;
            }
        }

        /// <summary>
        /// The DraggingMarkerStarted
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GMSMarkerEventEventArgs"/></param>
        private void DraggingMarkerStarted(object sender, GMSMarkerEventEventArgs e)
        {
            draggingPin = LookupPin(e.Marker);

            if (draggingPin != null)
            {
                UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
                Map.SendPinDragStart(draggingPin);
            }
        }

        /// <summary>
        /// The DraggingMarkerEnded
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GMSMarkerEventEventArgs"/></param>
        private void DraggingMarkerEnded(object sender, GMSMarkerEventEventArgs e)
        {
            if (draggingPin != null)
            {
                UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
                RefreshClusterItem();
                Map.SendPinDragEnd(draggingPin);
                draggingPin = null;
            }
        }

        /// <summary>
        /// The RefreshClusterItem
        /// </summary>
        private void RefreshClusterItem()
        {
            ClusteredMap.Pins.Remove(draggingPin);
            ClusteredMap.Pins.Add(draggingPin);
            clusterManager.Cluster();
        }

        /// <summary>
        /// The DraggingMarker
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GMSMarkerEventEventArgs"/></param>
        private void DraggingMarker(object sender, GMSMarkerEventEventArgs e)
        {
            if (draggingPin != null)
            {
                UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
                Map.SendPinDragging(draggingPin);
            }
        }

        /// <summary>
        /// The UpdatePositionWithoutMove
        /// </summary>
        /// <param name="pin">The pin<see cref="Pin"/></param>
        /// <param name="position">The position<see cref="Position"/></param>
        private void UpdatePositionWithoutMove(Pin pin, Position position)
        {
            try
            {
                withoutUpdateNative = true;
                pin.Position = position;
            }
            finally
            {
                withoutUpdateNative = false;
            }
        }

        /// <summary>
        /// The OnUpdateIconView
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        private void OnUpdateIconView(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View && outerItem?.Icon?.View != null)
            {
                NativeMap.InvokeOnMainThread(() =>
                {
                    var iconView = outerItem.Icon.View;
                    var nativeView = Utils.ConvertFormsToNative(iconView, new CGRect(0, 0, iconView.WidthRequest, iconView.HeightRequest));
                    nativeView.BackgroundColor = UIColor.Clear;
                    nativeItem.GroundAnchor = new CGPoint(iconView.AnchorX, iconView.AnchorY);
                    nativeItem.Icon = Utils.ConvertViewToImage(nativeView);
                });
            }
        }

        #endregion

        /// <summary>
        /// The GetItems
        /// </summary>
        /// <param name="map">The map<see cref="Map"/></param>
        /// <returns>The <see cref="IList{Pin}"/></returns>
        protected override IList<Pin> GetItems(Map map) => Map.Pins;

        /// <summary>
        /// The Register
        /// </summary>
        /// <param name="oldNativeMap">The oldNativeMap<see cref="MapView"/></param>
        /// <param name="oldMap">The oldMap<see cref="Map"/></param>
        /// <param name="newNativeMap">The newNativeMap<see cref="MapView"/></param>
        /// <param name="newMap">The newMap<see cref="Map"/></param>
        internal override void Register(MapView oldNativeMap, Map oldMap, MapView newNativeMap, Map newMap)
        {
            base.Register(oldNativeMap, oldMap, newNativeMap, newMap);

            var clusteredNewMap = (ClusteredMap)newMap;
            var algorithm = GetClusterAlgorithm(clusteredNewMap);

            var iconGenerator = new ClusterIconGeneratorHandler(ClusteredMap.ClusterOptions);
            clusterRenderer = new ClusterRendererHandler(newNativeMap, iconGenerator,
                ClusteredMap.ClusterOptions.MinimumClusterSize);
            clusterManager = new ClusterManager(newNativeMap, algorithm, clusterRenderer);

            ClusteredMap.OnCluster = HandleClusterRequest;

            if (newNativeMap == null) return;
            newNativeMap.InfoTapped += OnInfoTapped;
            newNativeMap.InfoLongPressed += OnInfoLongPressed;
            newNativeMap.TappedMarker = HandleGmsTappedMarker;
            newNativeMap.InfoClosed += InfoWindowClosed;
            newNativeMap.DraggingMarkerStarted += DraggingMarkerStarted;
            newNativeMap.DraggingMarkerEnded += DraggingMarkerEnded;
            newNativeMap.DraggingMarker += DraggingMarker;
        }

        /// <summary>
        /// The Unregister
        /// </summary>
        /// <param name="nativeMap">The nativeMap<see cref="MapView"/></param>
        /// <param name="map">The map<see cref="Map"/></param>
        internal override void Unregister(MapView nativeMap, Map map)
        {
            if (nativeMap != null)
            {
                nativeMap.DraggingMarker -= DraggingMarker;
                nativeMap.DraggingMarkerEnded -= DraggingMarkerEnded;
                nativeMap.DraggingMarkerStarted -= DraggingMarkerStarted;
                nativeMap.InfoClosed -= InfoWindowClosed;
                nativeMap.TappedMarker = null;
                nativeMap.InfoTapped -= OnInfoTapped;
            }

            ClusteredMap.OnCluster = null;

            base.Unregister(nativeMap, map);
        }

        /// <summary>
        /// The CreateNativeItem
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <returns>The <see cref="ClusteredMarker"/></returns>
        protected override ClusteredMarker CreateNativeItem(Pin outerItem)
        {
            var nativeMarker = new ClusteredMarker
            {
                Position = outerItem.Position.ToCoord(),
                Title = outerItem.Label,
                Snippet = outerItem.Address ?? string.Empty,
                Draggable = outerItem.IsDraggable,
                Rotation = outerItem.Rotation,
                GroundAnchor = new CGPoint(outerItem.Anchor.X, outerItem.Anchor.Y),
                InfoWindowAnchor = new CGPoint(outerItem.InfoWindowAnchor.X, outerItem.InfoWindowAnchor.Y),
                Flat = outerItem.Flat,
                ZIndex = outerItem.ZIndex,
                Opacity = 1f - outerItem.Transparency
            };

            if (outerItem.Icon != null)
            {
                var factory = imageFactory ?? DefaultImageFactory.Instance;
                nativeMarker.Icon = factory.ToUIImage(outerItem.Icon);
            }

            onMarkerCreating(outerItem, nativeMarker);

            outerItem.NativeObject = nativeMarker;

            clusterManager.AddItem(nativeMarker);
            itemsDictionary.Add(nativeMarker, outerItem);
            OnUpdateIconView(outerItem, nativeMarker);
            onMarkerCreated(outerItem, nativeMarker);

            return nativeMarker;
        }

        /// <summary>
        /// The DeleteNativeItem
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <returns>The <see cref="ClusteredMarker"/></returns>
        protected override ClusteredMarker DeleteNativeItem(Pin outerItem)
        {
            if (outerItem?.NativeObject == null)
                return null;
            var nativeMarker = outerItem.NativeObject as ClusteredMarker;

            onMarkerDeleting(outerItem, nativeMarker);

            nativeMarker.Map = null;

            clusterManager.RemoveItem(nativeMarker);

            if (ReferenceEquals(Map.SelectedPin, outerItem))
                Map.SelectedPin = null;

            itemsDictionary.Remove(nativeMarker);
            onMarkerDeleted(outerItem, nativeMarker);

            return nativeMarker;
        }

        /// <summary>
        /// The AddItems
        /// </summary>
        /// <param name="newItems">The newItems<see cref="IList"/></param>
        protected override void AddItems(IList newItems)
        {
            base.AddItems(newItems);
            clusterManager.Cluster();
        }

        /// <summary>
        /// The RemoveItems
        /// </summary>
        /// <param name="oldItems">The oldItems<see cref="IList"/></param>
        protected override void RemoveItems(IList oldItems)
        {
            base.RemoveItems(oldItems);
            clusterManager.Cluster();
        }

        /// <summary>
        /// The ResetItems
        /// </summary>
        protected override void ResetItems()
        {
            base.ResetItems();
            clusterManager.Cluster();
        }

        /// <summary>
        /// The OnItemPropertyChanged
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="PropertyChangedEventArgs"/></param>
        protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnItemPropertyChanged(sender, e);
            if (e.PropertyName != Pin.PositionProperty.PropertyName)
                clusterRenderer.SetUpdateMarker((ClusteredMarker)(sender as Pin)?.NativeObject);
        }

        /// <summary>
        /// The OnMapPropertyChanged
        /// </summary>
        /// <param name="e">The e<see cref="PropertyChangedEventArgs"/></param>
        internal override void OnMapPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Map.SelectedPinProperty.PropertyName)
            {
                if (!onMarkerEvent)
                    UpdateSelectedPin(Map.SelectedPin);
                Map.SendSelectedPinChanged(Map.SelectedPin);
            }
        }

        /// <summary>
        /// The OnUpdateAddress
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateAddress(Pin outerItem, ClusteredMarker nativeItem)
            => nativeItem.Snippet = outerItem.Address;

        /// <summary>
        /// The OnUpdateLabel
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateLabel(Pin outerItem, ClusteredMarker nativeItem)
            => nativeItem.Title = outerItem.Label;

        /// <summary>
        /// The OnUpdatePosition
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdatePosition(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (!withoutUpdateNative)
                nativeItem.Position = outerItem.Position.ToCoord();
        }

        /// <summary>
        /// The OnUpdateType
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateType(Pin outerItem, ClusteredMarker nativeItem)
        {
        }

        /// <summary>
        /// The OnUpdateIcon
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateIcon(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (outerItem.Icon.Type == BitmapDescriptorType.View)
                OnUpdateIconView(outerItem, nativeItem);
            else
            {
                if (nativeItem?.Icon != null)
                    nativeItem.Icon = DefaultImageFactory.Instance.ToUIImage(outerItem.Icon);
            }
        }

        /// <summary>
        /// The OnUpdateIsDraggable
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateIsDraggable(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Draggable = outerItem?.IsDraggable ?? false;
        }

        /// <summary>
        /// The OnUpdateRotation
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateRotation(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Rotation = outerItem?.Rotation ?? 0f;
        }

        /// <summary>
        /// The OnUpdateIsVisible
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateIsVisible(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (outerItem?.IsVisible ?? false)
            {
                nativeItem.Map = NativeMap;
            }
            else
            {
                nativeItem.Map = null;
                if (ReferenceEquals(Map.SelectedPin, outerItem))
                    Map.SelectedPin = null;
            }
        }

        /// <summary>
        /// The OnUpdateAnchor
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateAnchor(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.GroundAnchor = new CGPoint(outerItem.Anchor.X, outerItem.Anchor.Y);
        }

        /// <summary>
        /// The OnUpdateFlat
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateFlat(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Flat = outerItem.Flat;
        }

        /// <summary>
        /// The OnUpdateInfoWindowAnchor
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateInfoWindowAnchor(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.InfoWindowAnchor = new CGPoint(outerItem.Anchor.X, outerItem.Anchor.Y);
        }

        /// <summary>
        /// The OnUpdateZIndex
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateZIndex(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.ZIndex = outerItem.ZIndex;
        }

        /// <summary>
        /// The OnUpdateTransparency
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        protected override void OnUpdateTransparency(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Opacity = 1f - outerItem.Transparency;
        }
    }
}
