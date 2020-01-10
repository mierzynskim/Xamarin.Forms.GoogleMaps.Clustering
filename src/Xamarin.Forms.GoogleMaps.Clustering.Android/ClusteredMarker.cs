using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering;
using System;
using NativeBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;

namespace Xamarin.Forms.GoogleMaps.Clustering.Android
{
    /// <summary>
    /// Defines the <see cref="ClusteredMarker" />
    /// </summary>
    public class ClusteredMarker : Java.Lang.Object, IClusterItem
    {
        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredMarker"/> class.
        /// </summary>
        /// <param name="outerItem">The outerItem<see cref="Pin"/></param>
        public ClusteredMarker(Pin outerItem)
        {
            Id = Guid.NewGuid().ToString();
            Position = new LatLng(outerItem.Position.Latitude, outerItem.Position.Longitude);
            Title = outerItem.Label;
            Snippet = outerItem.Address;
            Draggable = outerItem.IsDraggable;
            Rotation = outerItem.Rotation;
            AnchorX = (float)outerItem.Anchor.X;
            AnchorY = (float)outerItem.Anchor.Y;
            InfoWindowAnchorX = (float)outerItem.InfoWindowAnchor.X;
            InfoWindowAnchorY = (float)outerItem.InfoWindowAnchor.Y;
            Flat = outerItem.Flat;
            Alpha = 1f - outerItem.Transparency;
            Visible = outerItem.IsVisible;
            ZIndex = outerItem.ZIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Position
        /// </summary>
        public LatLng Position { get; set; }

        /// <summary>
        /// Gets or sets the Alpha
        /// </summary>
        public float Alpha { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Draggable
        /// </summary>
        public bool Draggable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Flat
        /// </summary>
        public bool Flat { get; set; }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsInfoWindowShown
        /// </summary>
        public bool IsInfoWindowShown { get; set; }

        /// <summary>
        /// Gets or sets the Rotation
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the Snippet
        /// </summary>
        public string Snippet { get; set; }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Visible
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the AnchorX
        /// </summary>
        public float AnchorX { get; set; }

        /// <summary>
        /// Gets or sets the AnchorY
        /// </summary>
        public float AnchorY { get; set; }

        /// <summary>
        /// Gets or sets the InfoWindowAnchorX
        /// </summary>
        public float InfoWindowAnchorX { get; set; }

        /// <summary>
        /// Gets or sets the InfoWindowAnchorY
        /// </summary>
        public float InfoWindowAnchorY { get; set; }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        public NativeBitmapDescriptor Icon { get; set; }

        /// <summary>
        /// Gets or sets the ZIndex
        /// </summary>
        public int ZIndex { get; set; }

        #endregion
    }
}
