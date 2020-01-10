using CoreGraphics;
using Foundation;
using Google.Maps.Utility;
using System;
using UIKit;
using Xamarin.Forms.GoogleMaps.iOS.Factories;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    /// <summary>
    /// Defines the <see cref="ClusterIconGeneratorHandler" />
    /// </summary>
    internal class ClusterIconGeneratorHandler : DefaultClusterIconGenerator
    {
        #region Variables

        /// <summary>
        /// Defines the iconCache
        /// </summary>
        private readonly NSCache iconCache;

        /// <summary>
        /// Defines the options
        /// </summary>
        private readonly ClusterOptions options;

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterIconGeneratorHandler"/> class.
        /// </summary>
        /// <param name="options">The options<see cref="ClusterOptions"/></param>
        public ClusterIconGeneratorHandler(ClusterOptions options)
        {
            iconCache = new NSCache();
            this.options = options;
        }

        #endregion

        #region Public

        /// <summary>
        /// The IconForSize
        /// </summary>
        /// <param name="size">The size<see cref="nuint"/></param>
        /// <returns>The <see cref="UIImage"/></returns>
        public override UIImage IconForSize(nuint size)
        {
            string text = null;
            nuint bucketIndex = 0;

            if (options.EnableBuckets)
            {
                var buckets = options.Buckets;
                bucketIndex = BucketIndexForSize((nint)size);
                text = size < (nuint)buckets[0] ? size.ToString() : $"{buckets[bucketIndex]}+";
            }
            else text = size.ToString();

            if (options.RendererCallback != null)
                return DefaultImageFactory.Instance.ToUIImage(options.RendererCallback(text));
            if (options.RendererImage != null)
                return GetIconForText(text, DefaultImageFactory.Instance.ToUIImage(options.RendererImage));
            return GetIconForText(text, bucketIndex);
        }

        #endregion

        #region Private

        /// <summary>
        /// The BucketIndexForSize
        /// </summary>
        /// <param name="size">The size<see cref="nint"/></param>
        /// <returns>The <see cref="nuint"/></returns>
        private nuint BucketIndexForSize(nint size)
        {
            uint index = 0;
            var buckets = options.Buckets;

            while (index + 1 < buckets.Length && buckets[index + 1] <= size)
                ++index;

            return index;
        }

        /// <summary>
        /// The GetIconForText
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <param name="baseImage">The baseImage<see cref="UIImage"/></param>
        /// <returns>The <see cref="UIImage"/></returns>
        private UIImage GetIconForText(string text, UIImage baseImage)
        {
            var nsText = new NSString(text);
            var icon = iconCache.ObjectForKey(nsText);
            if (icon != null)
                return (UIImage)icon;

            var font = UIFont.BoldSystemFontOfSize(12);
            var size = baseImage.Size;
            UIGraphics.BeginImageContextWithOptions(size, false, 0.0f);
            baseImage.Draw(new CGRect(0, 0, size.Width, size.Height));
            var rect = new CGRect(0, 0, baseImage.Size.Width, baseImage.Size.Height);

            var paragraphStyle = NSParagraphStyle.Default;
            var attributes = new UIStringAttributes(NSDictionary.FromObjectsAndKeys(
                objects: new NSObject[] { font, paragraphStyle, options.RendererTextColor.ToUIColor() },
                keys: new NSObject[] { UIStringAttributeKey.Font, UIStringAttributeKey.ParagraphStyle, UIStringAttributeKey.ForegroundColor }
            ));

            var textSize = nsText.GetSizeUsingAttributes(attributes);
            var textRect = RectangleFExtensions.Inset(rect, (rect.Size.Width - textSize.Width) / 2,
                (rect.Size.Height - textSize.Height) / 2);
            nsText.DrawString(RectangleFExtensions.Integral(textRect), attributes);

            var newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            iconCache.SetObjectforKey(newImage, nsText);
            return newImage;
        }

        /// <summary>
        /// The GetIconForText
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <param name="bucketIndex">The bucketIndex<see cref="nuint"/></param>
        /// <returns>The <see cref="UIImage"/></returns>
        private UIImage GetIconForText(string text, nuint bucketIndex)
        {
            var nsText = new NSString(text);
            var icon = iconCache.ObjectForKey(nsText);
            if (icon != null)
                return (UIImage)icon;

            var font = UIFont.BoldSystemFontOfSize(14);
            var paragraphStyle = NSParagraphStyle.Default;
            var dict = NSDictionary.FromObjectsAndKeys(
                objects: new NSObject[] { font, paragraphStyle, options.RendererTextColor.ToUIColor() },
                keys: new NSObject[] { UIStringAttributeKey.Font, UIStringAttributeKey.ParagraphStyle, UIStringAttributeKey.ForegroundColor }
            );
            var attributes = new UIStringAttributes(dict);


            var textSize = nsText.GetSizeUsingAttributes(attributes);
            var rectDimension = Math.Max(20, Math.Max(textSize.Width, textSize.Height)) + 3 * bucketIndex + 6;
            var rect = new CGRect(0.0f, 0.0f, rectDimension, rectDimension);

            UIGraphics.BeginImageContext(rect.Size);
            UIGraphics.BeginImageContextWithOptions(rect.Size, false, 0);

            var ctx = UIGraphics.GetCurrentContext();
            ctx.SaveState();

            bucketIndex = (nuint)Math.Min((int)bucketIndex, options.BucketColors.Length - 1);
            var backColor = options.BucketColors[bucketIndex];
            ctx.SetFillColor(backColor.ToCGColor());
            ctx.FillEllipseInRect(rect);
            ctx.RestoreState();

            UIColor.White.SetColor();
            var textRect = rect.Inset((rect.Size.Width - textSize.Width) / 2,
                (rect.Size.Height - textSize.Height) / 2);
            nsText.DrawString(textRect.Integral(), attributes);

            var newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            iconCache.SetObjectforKey(newImage, nsText);

            return newImage;
        }

        #endregion
    }
}
