using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    /// <summary>
    /// Container of the cluster options
    /// </summary>
    public class ClusterOptions
    {
        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterOptions"/> class.
        /// </summary>
        public ClusterOptions()
        {
            Algorithm = ClusterAlgorithm.NonHierarchicalDistanceBased;
            EnableBuckets = true;
            RendererImage = null;
            RendererCallback = null;
            ResetBuckets();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Algorithm
        /// </summary>
        public ClusterAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Xamarin.Forms.GoogleMaps.Clustering.ClusterOptions"/> enable buckets grouping.
        /// </summary>
        public bool EnableBuckets { get; set; }

        /// <summary>
        /// Gets the Buckets
        /// </summary>
        public int[] Buckets { get; private set; }

        /// <summary>
        /// Gets the bucket colors.
        /// </summary>
        public Color[] BucketColors { get; private set; }

        /// <summary>
        /// Gets or sets the color of the renderer text.
        /// </summary>
        internal Color RendererTextColor { get; set; }

        /// <summary>
        /// Gets or sets the renderer image.
        /// </summary>
        internal BitmapDescriptor RendererImage { get; set; }

        /// <summary>
        /// Gets or sets the renderer callback.
        /// </summary>
        internal Func<string, BitmapDescriptor> RendererCallback { get; set; }

        /// <summary>
        /// Gets or sets the minimum cluster size.
        /// </summary>
        internal int MinimumClusterSize { get; set; } = 5;

        #endregion

        #region Public

        /// <summary>
        /// Sets the buckets.
        /// </summary>
        /// <param name="buckets">Buckets.</param>
        /// <param name="color">Use one color for all buckets.</param>
        public void SetBuckets(int[] buckets, Color color)
        {
            if (color == null)
                throw new ArgumentException("There most be at least one color specified for the buckets.");
            SetBuckets(buckets, (from bucket in buckets select color).ToArray());
        }

        /// <summary>
        /// Sets the buckets.
        /// </summary>
        /// <param name="buckets">Buckets.</param>
        /// <param name="colors">Colors.</param>
        public void SetBuckets(int[] buckets, Color[] colors)
        {
            if (buckets == null && !buckets.Any())
                throw new ArgumentException("There must be at least one buckets as a parameter.");
            if (buckets.Length != colors.Length)
                throw new ArgumentException("The buckets length must be equal to the colors length.");

            Buckets = buckets;
            BucketColors = colors;
        }

        /// <summary>
        /// Sets the buckets.
        /// </summary>
        /// <param name="buckets">Buckets.</param>
        public void SetBuckets(Dictionary<int, Color> buckets)
        {
            Buckets = buckets.Keys.ToArray();
            BucketColors = buckets.Values.ToArray();
        }

        /// <summary>
        /// Resets the buckets.
        /// </summary>
        public void ResetBuckets()
        {
            Buckets = new int[] { 10, 50, 100, 200, 1000 };
            BucketColors = new Color[]
            {
                Color.FromHex("#0099cc"),
                Color.FromHex("#669900"),
                Color.FromHex("#ff8800"),
                Color.FromHex("#cc0000"),
                Color.FromHex("#9933cc")
            };

            RendererTextColor = Color.White;
            RendererImage = null;
            RendererCallback = null;
        }

        /// <summary>
        /// Sets the rendering method to use default colors.
        /// </summary>
        public void SetRenderUsingColors()
        {
            SetRenderUsingColors(Color.White);
        }

        /// <summary>
        /// Sets the rendering method to use default colors.
        /// </summary>
        /// <param name="textColor">The text color.</param>
        public void SetRenderUsingColors(Color textColor)
        {
            RendererTextColor = textColor;
            RendererImage = null;
            RendererCallback = null;
        }

        /// <summary>
        /// Sets the rendering method to use custom bitmap image.
        /// </summary>
        /// <param name="image">The BitmapDescriptor image. The number of items will be drawn automatically on top of this image.</param>
        public void SetRenderUsingImage(BitmapDescriptor image)
        {
            SetRenderUsingImage(image, Color.White);
        }

        /// <summary>
        /// Sets the rendering method to use custom bitmap image.
        /// </summary>
        /// <param name="image">The BitmapDescriptor image. The number of items will be drawn automatically on top of this image.</param>
        /// <param name="textColor">The text color.</param>
        public void SetRenderUsingImage(BitmapDescriptor image, Color textColor)
        {
            RendererTextColor = textColor;
            RendererImage = image;
            RendererCallback = null;
        }

        /// <summary>
        /// Sets the rendering method to use custom lambda action.
        /// </summary>
        /// <param name="callback">A callback which recieves the value to display (a string or bucket value) and renders it into a BitmapDescriptor.</param>
        public void SetRenderUsingCustomAction(Func<string, BitmapDescriptor> callback)
        {
            RendererImage = null;
            RendererCallback = callback;
        }

        /// <summary>
        /// Sets the minimum cluster size. Clusters with less will be broken-up into single-item clusters
        /// Default value is 5.
        /// </summary>
        /// <param name="size"></param>
        public void SetMinimumClusterSize(int size)
        {
            MinimumClusterSize = size;
        }

        #endregion
    }
}
