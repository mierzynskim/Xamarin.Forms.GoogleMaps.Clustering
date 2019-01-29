using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.Algo;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using Xamarin.Forms.GoogleMaps.Clustering.Android.Cluster;
using Xamarin.Forms.GoogleMaps.Logics;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    internal class ClusterLogic : DefaultPinLogic<ClusteredMarker, GoogleMap>
    {
        protected override IList<Pin> GetItems(Map map) => (map as ClusteredMap)?.ClusteredPins;

        private volatile bool _onMarkerEvent = false;
        private Pin _draggingPin;
        private volatile bool _withoutUpdateNative = false;
        
        private ClusterManager _clusterManager;
        private ClusterLogicHandler _clusterHandler;

        private readonly Context _context;
        protected readonly IBitmapDescriptorFactory _bitmapDescriptorFactory;
        private readonly Action<Pin, MarkerOptions> _onMarkerCreating;
        private readonly Action<Pin, ClusteredMarker> _onMarkerCreated;
        private readonly Action<Pin, ClusteredMarker> _onMarkerDeleting;
        private readonly Action<Pin, ClusteredMarker> _onMarkerDeleted;

        public ClusteredMap ClusteredMap => (ClusteredMap) Map;

        public ClusterLogic(
            Context context,
            IBitmapDescriptorFactory bitmapDescriptorFactory,
            Action<Pin, MarkerOptions> onMarkerCreating,
            Action<Pin, ClusteredMarker> onMarkerCreated, 
            Action<Pin, ClusteredMarker> onMarkerDeleting,
            Action<Pin, ClusteredMarker> onMarkerDeleted)
        {
            _bitmapDescriptorFactory = bitmapDescriptorFactory;
            _context = context;
            _onMarkerCreating = onMarkerCreating;
            _onMarkerCreated = onMarkerCreated;
            _onMarkerDeleting = onMarkerDeleting;
            _onMarkerDeleted = onMarkerDeleted;
        }

        internal override void Register(GoogleMap oldNativeMap, Map oldMap, GoogleMap newNativeMap, Map newMap)
        {
            base.Register(oldNativeMap, oldMap, newNativeMap, newMap);

            this._clusterManager = new ClusterManager(Xamarin.Forms.Forms.Context, this.NativeMap);
            this._clusterHandler = new ClusterLogicHandler(this.Map, this._clusterManager, this);

            switch (this.ClusteredMap.ClusterOptions.Algorithm)
            {
                case ClusterAlgorithm.GridBased:
                    this._clusterManager.Algorithm = new GridBasedAlgorithm();
                    break;
                case ClusterAlgorithm.VisibleNonHierarchicalDistanceBased:
                    this._clusterManager.Algorithm =
                        new NonHierarchicalViewBasedAlgorithm(
                            _context.Resources.DisplayMetrics.WidthPixels,
                            _context.Resources.DisplayMetrics.HeightPixels);
                    break;
                case ClusterAlgorithm.NonHierarchicalDistanceBased:
                    this._clusterManager.Algorithm =
                        new NonHierarchicalDistanceBasedAlgorithm();
                    break;
            }

            if (newNativeMap != null)
            {
                this.ClusteredMap.OnCluster = HandleClusterRequest;

                this.NativeMap.SetOnCameraIdleListener(this._clusterManager);
                this.NativeMap.SetOnMarkerClickListener(this._clusterManager);
                this.NativeMap.SetOnInfoWindowClickListener(this._clusterManager);

                this._clusterManager.Renderer = new XamarinClusterRenderer(this._context,
                    this.ClusteredMap,
                    this.NativeMap,
                this._clusterManager);

                this._clusterManager.SetOnClusterClickListener(this._clusterHandler);
                this._clusterManager.SetOnClusterInfoWindowClickListener(this._clusterHandler);
                this._clusterManager.SetOnClusterItemClickListener(this._clusterHandler);
                this._clusterManager.SetOnClusterItemInfoWindowClickListener(this._clusterHandler);
            }
        }

        internal override void Unregister(GoogleMap nativeMap, Map map)
        {
            if (nativeMap != null)
            {
                this.NativeMap.SetOnCameraChangeListener(null);
                this.NativeMap.SetOnMarkerClickListener(null);
                this.NativeMap.SetOnInfoWindowClickListener(null);

                this._clusterHandler.Dispose();
                this._clusterManager.Dispose();
            }

            base.Unregister(nativeMap, map);
        }

        protected override ClusteredMarker CreateNativeItem(Pin outerItem)
        {
            var opts = new MarkerOptions()
                .SetPosition(new LatLng(outerItem.Position.Latitude, outerItem.Position.Longitude))
                .SetTitle(outerItem.Label)
                .SetSnippet(outerItem.Address)
                .SetSnippet(outerItem.Address)
                .Draggable(outerItem.IsDraggable)
                .SetRotation(outerItem.Rotation)
                .Anchor((float)outerItem.Anchor.X, (float)outerItem.Anchor.Y)
                .InvokeZIndex(outerItem.ZIndex)
                .Flat(outerItem.Flat)
                .SetAlpha(1f - outerItem.Transparency);
            
            if (outerItem.Icon != null)
            {
                var factory = _bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon);
                opts.SetIcon(nativeDescriptor);
            }

            _onMarkerCreating(outerItem, opts);

            var marker = new ClusteredMarker(outerItem);
            if (outerItem.Icon != null)
            {
                var factory = _bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon);
                marker.Icon = nativeDescriptor;
            }
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View)
            {
                marker.Visible = false; 
                TransformXamarinViewToAndroidBitmap(outerItem, marker);
            }
            else
            {
                marker.Visible = outerItem.IsVisible;
            }

            outerItem.NativeObject = marker;
            _onMarkerCreated(outerItem, marker);
            this._clusterManager.AddItem(marker);
            return marker;
        }

        protected override ClusteredMarker DeleteNativeItem(Pin outerItem)
        {
            var marker = outerItem.NativeObject as ClusteredMarker;
            if (marker == null)
                return null;
            _onMarkerDeleting(outerItem, marker);
            this._clusterManager.RemoveItem(marker);
            outerItem.NativeObject = null;

            if (ReferenceEquals(Map.SelectedPin, outerItem))
                Map.SelectedPin = null;

            _onMarkerDeleted(outerItem, marker);
            return marker;
        }

        public Pin LookupPin(ClusteredMarker marker)
        {
            return GetItems(Map).FirstOrDefault(outerItem => ((ClusteredMarker)outerItem.NativeObject).Id == marker.Id);
        }
        
        public void HandleClusterRequest()
        {
            this._clusterManager.Cluster();
        }

        internal override void OnMapPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Map.SelectedPinProperty.PropertyName)
            {
                if (!_onMarkerEvent)
                    UpdateSelectedPin(Map.SelectedPin);
                Map.SendSelectedPinChanged(Map.SelectedPin);
            }
        }

        void UpdateSelectedPin(Pin pin)
        {
            if (pin == null)
            {
                foreach (var outerItem in GetItems(Map))
                {
                    (outerItem.NativeObject as Marker)?.HideInfoWindow();
                }
            }
            else
            {
                var targetPin = LookupPin(pin.NativeObject as ClusteredMarker);
                (targetPin?.NativeObject as Marker)?.ShowInfoWindow();
            }
        }

        void UpdatePositionWithoutMove(Pin pin, Position position)
        {
            try
            {
                _withoutUpdateNative = true;
                pin.Position = position;
            }
            finally
            {
                _withoutUpdateNative = false;
            }
        }

        protected override void OnUpdateAddress(Pin outerItem, ClusteredMarker nativeItem)
            => nativeItem.Snippet = outerItem.Address;

        protected override void OnUpdateLabel(Pin outerItem, ClusteredMarker nativeItem)
            => nativeItem.Title = outerItem.Label;

        protected override void OnUpdatePosition(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (!_withoutUpdateNative)
            {
                nativeItem.Position = outerItem.Position.ToLatLng();
            }
        }

        protected override void OnUpdateType(Pin outerItem, ClusteredMarker nativeItem)
        {
        }

        protected override void OnUpdateIcon(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (outerItem.Icon != null && outerItem.Icon.Type == BitmapDescriptorType.View)
            {
                TransformXamarinViewToAndroidBitmap(outerItem, nativeItem);
            }
            else
            {
                var factory = _bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon);
                nativeItem.Icon = nativeDescriptor;
                nativeItem.AnchorX = 0.5f;
                nativeItem.AnchorY = 0.5f;
                //nativeItem.SetInfoWindowAnchor(0.5f, 0f);
            }
        }

        private async void TransformXamarinViewToAndroidBitmap(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View && outerItem.Icon?.View != null)
            {
                var iconView = outerItem.Icon.View;
                var nativeView = await Utils.ConvertFormsToNative(
                    iconView, 
                    new Rectangle(0, 0, (double)Utils.DpToPx((float)iconView.WidthRequest), (double)Utils.DpToPx((float)iconView.HeightRequest)), 
                    Platform.Android.Platform.CreateRendererWithContext(iconView, _context));
                var otherView = new FrameLayout(nativeView.Context);
                nativeView.LayoutParameters = new FrameLayout.LayoutParams(Utils.DpToPx((float)iconView.WidthRequest), Utils.DpToPx((float)iconView.HeightRequest));
                otherView.AddView(nativeView);
                nativeItem.Icon = await Utils.ConvertViewToBitmapDescriptor(otherView);
                nativeItem.AnchorX = (float) iconView.AnchorX;
                nativeItem.AnchorY = (float)iconView.AnchorY;
                nativeItem.Visible = true;
            }
        }

        protected override void OnUpdateIsDraggable(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Draggable = outerItem?.IsDraggable ?? false;
        }

        protected override void OnUpdateRotation(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Rotation = outerItem?.Rotation ?? 0f;
        }

        protected override void OnUpdateIsVisible(Pin outerItem, ClusteredMarker nativeItem)
        {
            var isVisible = outerItem?.IsVisible ?? false;
            nativeItem.Visible = isVisible;

            if (!isVisible && ReferenceEquals(Map.SelectedPin, outerItem))
            {
                Map.SelectedPin = null;
            }
        }
        protected override void OnUpdateAnchor(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.AnchorX = (float)outerItem.Anchor.X;
            nativeItem.AnchorY = (float)outerItem.Anchor.Y;
        }

        protected override void OnUpdateFlat(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Flat = outerItem.Flat;
        }

        protected override void OnUpdateInfoWindowAnchor(Pin outerItem, ClusteredMarker nativeItem)
        {
            //nativeItem.SetInfoWindowAnchor((float) outerItem.InfoWindowAnchor.X, (float) outerItem.InfoWindowAnchor.Y);
        }

        protected override void OnUpdateZIndex(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.ZIndex = outerItem.ZIndex;
        }

        protected override void OnUpdateTransparency(Pin outerItem, ClusteredMarker nativeItem)
        {
            nativeItem.Alpha = 1f - outerItem.Transparency;
        }
    }
}