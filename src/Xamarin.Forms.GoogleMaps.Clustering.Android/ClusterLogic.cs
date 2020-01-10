using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.Algo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps.Android.Extensions;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using Xamarin.Forms.GoogleMaps.Logics;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    /// <summary>
    /// Defines the <see cref="ClusterLogic" />
    /// </summary>
    internal class ClusterLogic : DefaultPinLogic<ClusteredMarker, GoogleMap>
    {
        #region Variables

        /// <summary>
        /// Defines the onMarkerEvent
        /// </summary>
        private volatile bool onMarkerEvent = false;

        /// <summary>
        /// Defines the draggingPin
        /// </summary>
        private Pin draggingPin;

        /// <summary>
        /// Defines the withoutUpdateNative
        /// </summary>
        private volatile bool withoutUpdateNative;

        /// <summary>
        /// Defines the clusterManager
        /// </summary>
        private ClusterManager clusterManager;

        /// <summary>
        /// Defines the clusterHandler
        /// </summary>
        private ClusterLogicHandler clusterHandler;

        /// <summary>
        /// Defines the context
        /// </summary>
        private readonly Activity context;

        /// <summary>
        /// Defines the bitmapDescriptorFactory
        /// </summary>
        private readonly IBitmapDescriptorFactory bitmapDescriptorFactory;

        /// <summary>
        /// Defines the onMarkerCreating
        /// </summary>
        private readonly Action<Pin, MarkerOptions> onMarkerCreating;

        /// <summary>
        /// Defines the onMarkerCreated
        /// </summary>
        private readonly Action<Pin, ClusteredMarker> onMarkerCreated;

        /// <summary>
        /// Defines the onMarkerDeleting
        /// </summary>
        private readonly Action<Pin, ClusteredMarker> onMarkerDeleting;

        /// <summary>
        /// Defines the onMarkerDeleted
        /// </summary>
        private readonly Action<Pin, ClusteredMarker> onMarkerDeleted;

        /// <summary>
        /// Defines the previousCameraPostion
        /// </summary>
        private global::Android.Gms.Maps.Model.CameraPosition previousCameraPostion;

        /// <summary>
        /// Defines the clusterRenderer
        /// </summary>
        private ClusterRenderer clusterRenderer;

        /// <summary>
        /// Defines the itemsDictionary
        /// </summary>
        private readonly Dictionary<string, Pin> itemsDictionary = new Dictionary<string, Pin>();

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterLogic"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Activity"/></param>
        /// <param name="bitmapDescriptorFactory">The bitmapDescriptorFactory<see cref="IBitmapDescriptorFactory"/></param>
        /// <param name="onMarkerCreating">The onMarkerCreating<see cref="Action{Pin, MarkerOptions}"/></param>
        /// <param name="onMarkerCreated">The onMarkerCreated<see cref="Action{Pin, ClusteredMarker}"/></param>
        /// <param name="onMarkerDeleting">The onMarkerDeleting<see cref="Action{Pin, ClusteredMarker}"/></param>
        /// <param name="onMarkerDeleted">The onMarkerDeleted<see cref="Action{Pin, ClusteredMarker}"/></param>
        public ClusterLogic(
            Activity context,
            IBitmapDescriptorFactory bitmapDescriptorFactory,
            Action<Pin, MarkerOptions> onMarkerCreating,
            Action<Pin, ClusteredMarker> onMarkerCreated,
            Action<Pin, ClusteredMarker> onMarkerDeleting,
            Action<Pin, ClusteredMarker> onMarkerDeleted)
        {
            this.bitmapDescriptorFactory = bitmapDescriptorFactory;
            this.context = context;
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
        public ClusteredMap ClusteredMap => (ClusteredMap)Map;

        #endregion

        #region Public

        /// <summary>
        /// The LookupPin
        /// </summary>
        /// <param name="marker">The marker<see cref="ClusteredMarker"/></param>
        /// <returns>The <see cref="Pin"/></returns>
        public Pin LookupPin(ClusteredMarker marker)
        {
            var markerId = marker.Id;
            return markerId != null ? itemsDictionary[markerId] : null;
        }

        /// <summary>
        /// The HandleClusterRequest
        /// </summary>
        public void HandleClusterRequest()
        {
            clusterManager.Cluster();
        }

        #endregion

        #region Private

        /// <summary>
        /// The GetClusterAlgorithm
        /// </summary>
        /// <returns>The <see cref="IAlgorithm"/></returns>
        private IAlgorithm GetClusterAlgorithm()
        {
            IAlgorithm algorithm;
            switch (ClusteredMap.ClusterOptions.Algorithm)
            {
                case ClusterAlgorithm.GridBased:
                    algorithm = new GridBasedAlgorithm();
                    break;
                case ClusterAlgorithm.VisibleNonHierarchicalDistanceBased:
                    algorithm =
                        new NonHierarchicalViewBasedAlgorithm(
                            context.Resources.DisplayMetrics.WidthPixels,
                            context.Resources.DisplayMetrics.HeightPixels);
                    break;
                default:
                    algorithm =
                        new NonHierarchicalDistanceBasedAlgorithm();
                    break;
            }

            return algorithm;
        }

        /// <summary>
        /// The NativeMapOnCameraIdle
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void NativeMapOnCameraIdle(object sender, EventArgs e)
        {
            var cameraPosition = NativeMap.CameraPosition;
            if (previousCameraPostion == null || Math.Abs(previousCameraPostion.Zoom - cameraPosition.Zoom) > 0.001)
            {
                previousCameraPostion = NativeMap.CameraPosition;
                clusterManager.Cluster();
            }
        }

        /// <summary>
        /// The AddMarker
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="marker">The marker<see cref="ClusteredMarker"/></param>
        private void AddMarker(Pin outerItem, ClusteredMarker marker)
        {
            outerItem.NativeObject = marker;
            onMarkerCreated(outerItem, marker);
            clusterManager.AddItem(marker);
            itemsDictionary.Add(marker.Id, outerItem);
        }

        /// <summary>
        /// The UpdateSelectedPin
        /// </summary>
        /// <param name="pin">The pin<see cref="Pin"/></param>
        private void UpdateSelectedPin(Pin pin)
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
        /// The TransformXamarinViewToAndroidBitmap
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <param name="nativeItem">The nativeItem<see cref="ClusteredMarker"/></param>
        private async void TransformXamarinViewToAndroidBitmap(Pin outerItem, ClusteredMarker nativeItem)
        {
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View && outerItem.Icon?.View != null)
            {
                var iconView = outerItem.Icon.View;
                var nativeView = await Utils.ConvertFormsToNative(
                    iconView,
                    new Rectangle(0, 0, (double)Utils.DpToPx((float)iconView.WidthRequest), (double)Utils.DpToPx((float)iconView.HeightRequest)),
                    Platform.Android.Platform.CreateRendererWithContext(iconView, context));
                var otherView = new FrameLayout(nativeView.Context);
                nativeView.LayoutParameters = new FrameLayout.LayoutParams(Utils.DpToPx((float)iconView.WidthRequest), Utils.DpToPx((float)iconView.HeightRequest));
                otherView.AddView(nativeView);
                nativeItem.Icon = await Utils.ConvertViewToBitmapDescriptor(otherView);
                nativeItem.AnchorX = (float)iconView.AnchorX;
                nativeItem.AnchorY = (float)iconView.AnchorY;
                nativeItem.Visible = true;
                if (outerItem.NativeObject == null)
                    AddMarker(outerItem, nativeItem);
            }
        }

        /// <summary>
        /// The OnMarkerDragStart
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GoogleMap.MarkerDragStartEventArgs"/></param>
        private void OnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
        {
            var clusterItem = clusterRenderer.GetClusterItem(e.Marker);
            draggingPin = LookupPin((ClusteredMarker)clusterItem);

            if (draggingPin == null) return;
            UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
            Map.SendPinDragStart(draggingPin);
        }

        /// <summary>
        /// The OnMarkerDrag
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GoogleMap.MarkerDragEventArgs"/></param>
        private void OnMarkerDrag(object sender, GoogleMap.MarkerDragEventArgs e)
        {
            if (draggingPin == null) return;
            UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
            Map.SendPinDragging(draggingPin);
        }

        /// <summary>
        /// The OnMarkerDragEnd
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="GoogleMap.MarkerDragEndEventArgs"/></param>
        private void OnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            if (draggingPin != null)
            {
                UpdatePositionWithoutMove(draggingPin, e.Marker.Position.ToPosition());
                RefreshClusterItem();
                Map.SendPinDragEnd(draggingPin);
                draggingPin = null;
            }
            withoutUpdateNative = false;
        }

        /// <summary>
        /// The RefreshClusterItem
        /// </summary>
        private void RefreshClusterItem()
        {
            clusterManager.RemoveItem((Java.Lang.Object)draggingPin.NativeObject);
            clusterManager.AddItem(CreateNativeItem(draggingPin));
            clusterManager.Cluster();
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
        /// <param name="oldNativeMap">The oldNativeMap<see cref="GoogleMap"/></param>
        /// <param name="oldMap">The oldMap<see cref="Map"/></param>
        /// <param name="newNativeMap">The newNativeMap<see cref="GoogleMap"/></param>
        /// <param name="newMap">The newMap<see cref="Map"/></param>
        internal override void Register(GoogleMap oldNativeMap, Map oldMap, GoogleMap newNativeMap, Map newMap)
        {
            base.Register(oldNativeMap, oldMap, newNativeMap, newMap);

            clusterManager = new ClusterManager(context, NativeMap) { Algorithm = GetClusterAlgorithm() };
            clusterHandler = new ClusterLogicHandler(ClusteredMap, clusterManager, this);

            if (newNativeMap == null) return;
            ClusteredMap.OnCluster = HandleClusterRequest;

            NativeMap.CameraIdle += NativeMapOnCameraIdle;
            NativeMap.SetOnMarkerClickListener(clusterManager);
            NativeMap.SetOnInfoWindowClickListener(clusterManager);
            newNativeMap.MarkerDragStart += OnMarkerDragStart;
            newNativeMap.MarkerDragEnd += OnMarkerDragEnd;
            newNativeMap.MarkerDrag += OnMarkerDrag;

            clusterRenderer = new ClusterRenderer(context,
                ClusteredMap,
                NativeMap,
                clusterManager);
            clusterManager.Renderer = clusterRenderer;

            clusterManager.SetOnClusterClickListener(clusterHandler);
            clusterManager.SetOnClusterInfoWindowClickListener(clusterHandler);
            clusterManager.SetOnClusterItemClickListener(clusterHandler);
            clusterManager.SetOnClusterItemInfoWindowClickListener(clusterHandler);
        }

        /// <summary>
        /// The Unregister
        /// </summary>
        /// <param name="nativeMap">The nativeMap<see cref="GoogleMap"/></param>
        /// <param name="map">The map<see cref="Map"/></param>
        internal override void Unregister(GoogleMap nativeMap, Map map)
        {
            if (nativeMap != null)
            {
                NativeMap.CameraIdle -= NativeMapOnCameraIdle;
                NativeMap.SetOnMarkerClickListener(null);
                NativeMap.SetOnInfoWindowClickListener(null);
                nativeMap.MarkerDrag -= OnMarkerDrag;
                nativeMap.MarkerDragEnd -= OnMarkerDragEnd;
                nativeMap.MarkerDragStart -= OnMarkerDragStart;

                clusterHandler.Dispose();
                clusterManager.Dispose();
            }

            base.Unregister(nativeMap, map);
        }

        /// <summary>
        /// The CreateNativeItem
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <returns>The <see cref="ClusteredMarker"/></returns>
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
                var factory = bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon);
                opts.SetIcon(nativeDescriptor);
            }

            onMarkerCreating(outerItem, opts);

            var marker = new ClusteredMarker(outerItem);
            if (outerItem.Icon != null)
            {
                var factory = bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon);
                marker.Icon = nativeDescriptor;
            }
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View)
            {
                marker.Visible = false;
                TransformXamarinViewToAndroidBitmap(outerItem, marker);
                return marker;
            }
            else
            {
                marker.Visible = outerItem.IsVisible;
            }

            AddMarker(outerItem, marker);
            return marker;
        }

        /// <summary>
        /// The DeleteNativeItem
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        /// <returns>The <see cref="ClusteredMarker"/></returns>
        protected override ClusteredMarker DeleteNativeItem(Pin outerItem)
        {
            var marker = outerItem.NativeObject as ClusteredMarker;
            if (marker == null)
                return null;
            onMarkerDeleting(outerItem, marker);
            clusterManager.RemoveItem(marker);
            outerItem.NativeObject = null;

            if (ReferenceEquals(Map.SelectedPin, outerItem))
                Map.SelectedPin = null;

            itemsDictionary.Remove(marker.Id);
            onMarkerDeleted(outerItem, marker);
            return marker;
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
                clusterRenderer.SetUpdateMarker((sender as Pin).NativeObject as ClusteredMarker);
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
            {
                nativeItem.Position = outerItem.Position.ToLatLng();
            }
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
            if (outerItem.Icon != null && outerItem.Icon.Type == BitmapDescriptorType.View)
            {
                TransformXamarinViewToAndroidBitmap(outerItem, nativeItem);
            }
            else
            {
                var factory = bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon);
                nativeItem.Icon = nativeDescriptor;
                nativeItem.AnchorX = 0.5f;
                nativeItem.AnchorY = 0.5f;
                nativeItem.InfoWindowAnchorX = 0.5f;
                nativeItem.InfoWindowAnchorY = 0.5f;
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
            var isVisible = outerItem?.IsVisible ?? false;
            nativeItem.Visible = isVisible;

            if (!isVisible && ReferenceEquals(Map.SelectedPin, outerItem))
            {
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
            nativeItem.AnchorX = (float)outerItem.Anchor.X;
            nativeItem.AnchorY = (float)outerItem.Anchor.Y;
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
            nativeItem.InfoWindowAnchorX = (float)outerItem.InfoWindowAnchor.X;
            nativeItem.InfoWindowAnchorY = (float)outerItem.InfoWindowAnchor.Y;
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
            nativeItem.Alpha = 1f - outerItem.Transparency;
        }
    }
}
