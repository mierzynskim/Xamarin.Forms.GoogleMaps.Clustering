using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using Google.Maps;
using Google.Maps.Utility;
using UIKit;
using Xamarin.Forms.GoogleMaps.iOS.Extensions;
using Xamarin.Forms.GoogleMaps.iOS.Factories;
using Xamarin.Forms.GoogleMaps.Logics;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    internal class ClusterLogic : DefaultPinLogic<ClusteredMarker, MapView>
    {
        protected override IList<Pin> GetItems(Map map) => Map.Pins;

        private ClusteredMap ClusteredMap => (ClusteredMap) Map;

        private ClusterManager clusterManager;

        private bool onMarkerEvent;
        private Pin draggingPin;
        private volatile bool withoutUpdateNative;

        private readonly Action<Pin, Marker> onMarkerCreating;
        private readonly Action<Pin, Marker> onMarkerCreated;
        private readonly Action<Pin, Marker> onMarkerDeleting;
        private readonly Action<Pin, Marker> onMarkerDeleted;
        private readonly IImageFactory imageFactory;
        private ClusterRendererHandler clusterRenderer;
        
        private readonly Dictionary<NSObject, Pin> itemsDictionary = new Dictionary<NSObject, Pin>();

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

        internal override void Register(MapView oldNativeMap, Map oldMap, MapView newNativeMap, Map newMap)
        {
            base.Register(oldNativeMap, oldMap, newNativeMap, newMap);

            var clusteredNewMap = (ClusteredMap) newMap;
            var algorithm = GetClusterAlgorithm(clusteredNewMap);

            var iconGenerator = new ClusterIconGeneratorHandler(ClusteredMap.ClusterOptions);
            clusterRenderer = new ClusterRendererHandler(newNativeMap, iconGenerator);
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

        protected override void AddItems(IList newItems)
        {
            base.AddItems(newItems);
            clusterManager.Cluster();
        }

        protected override void RemoveItems(IList oldItems)
        {
            base.RemoveItems(oldItems);
            clusterManager.Cluster();
        }
        
        protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnItemPropertyChanged(sender, e);
            if (e.PropertyName != Pin.PositionProperty.PropertyName)
                clusterRenderer.SetUpdateMarker((ClusteredMarker) (sender as Pin)?.NativeObject);
        }

        internal override void OnMapPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Map.SelectedPinProperty.PropertyName)
            {
                if (!onMarkerEvent)
                    UpdateSelectedPin(Map.SelectedPin);
                Map.SendSelectedPinChanged(Map.SelectedPin);
            }
        }

        private void UpdateSelectedPin(Pin pin)
        {
            if (pin != null)
                NativeMap.SelectedMarker = (ClusteredMarker)pin.NativeObject;
            else
                NativeMap.SelectedMarker = null;
        }

        private Pin LookupPin(Marker marker)
        {
            var associatedClusteredMarker = marker.UserData;
            return associatedClusteredMarker != null ? itemsDictionary[associatedClusteredMarker] : null;
        }

        private void HandleClusterRequest()
        {
            clusterManager.Cluster();
        }

        private void OnInfoTapped(object sender, GMSMarkerEventEventArgs e)
        {
            var targetPin = LookupPin(e.Marker);

            targetPin?.SendTap();

            if (targetPin != null)
            {
                Map.SendInfoWindowClicked(targetPin);
            }
        }

        private void OnInfoLongPressed(object sender, GMSMarkerEventEventArgs e)
        {
            var targetPin = LookupPin(e.Marker);
            
            if (targetPin != null)
                Map.SendInfoWindowLongClicked(targetPin);
        }

        private bool HandleGmsTappedMarker(MapView mapView, Marker marker)
        {
            if (marker?.UserData is ICluster cluster)
            {
                var pins = GetClusterPins(cluster);
                return ClusteredMap.SendClusterClicked((int) cluster.Count, pins);
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

        private List<Pin> GetClusterPins(ICluster cluster)
        {
            var pins = new List<Pin>();
            foreach (var item in cluster.Items)
            {
                var clusterItem = (ClusteredMarker) item;
                pins.Add(itemsDictionary[clusterItem]);
            }

            return pins;
        }

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

        private void DraggingMarkerStarted(object sender, GMSMarkerEventEventArgs e)
        {
            draggingPin = LookupPin(e.Marker);

            if (draggingPin != null)
            {
                UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
                Map.SendPinDragStart(draggingPin);
            }
        }

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

        private void RefreshClusterItem()
        {
            ClusteredMap.Pins.Remove(draggingPin);
            ClusteredMap.Pins.Add(draggingPin);
            clusterManager.Cluster();
        }

        private void DraggingMarker(object sender, GMSMarkerEventEventArgs e)
        {
            if (draggingPin != null)
            {
                UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
                Map.SendPinDragging(draggingPin);
            }
        }

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

        protected override void OnUpdateAddress(Pin outerItem, ClusteredMarker nativeItem)
            => nativeItem.Snippet = outerItem.Address;

        protected override void OnUpdateLabel(Pin outerItem, ClusteredMarker nativeItem)
            => nativeItem.Title = outerItem.Label;

        protected override void OnUpdatePosition(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (!withoutUpdateNative)
                nativeItem.Position = outerItem.Position.ToCoord();
        }

        protected override void OnUpdateType(Pin outerItem, ClusteredMarker nativeItem)
        {
        }

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

        protected override void OnUpdateIsDraggable(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Draggable = outerItem?.IsDraggable ?? false;
        }

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

        protected override void OnUpdateRotation(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Rotation = outerItem?.Rotation ?? 0f;
        }

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

        protected override void OnUpdateAnchor(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.GroundAnchor = new CGPoint(outerItem.Anchor.X, outerItem.Anchor.Y);
        }

        protected override void OnUpdateFlat(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Flat = outerItem.Flat;
        }

        protected override void OnUpdateInfoWindowAnchor(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.InfoWindowAnchor = new CGPoint(outerItem.Anchor.X, outerItem.Anchor.Y);
        }

        protected override void OnUpdateZIndex(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.ZIndex = outerItem.ZIndex;
        }

        protected override void OnUpdateTransparency(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Opacity = 1f - outerItem.Transparency;
        }
    }
}