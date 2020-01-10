using System.Linq;
using Xamarin.Forms.GoogleMaps.iOS;
using Xamarin.Forms.GoogleMaps.Logics.iOS;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    /// <summary>
    /// Defines the <see cref="ClusteredMapRenderer" />
    /// </summary>
    public class ClusteredMapRenderer : MapRenderer
    {
        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredMapRenderer"/> class.
        /// </summary>
        public ClusteredMapRenderer()
        {
            Logics.Remove(Logics.OfType<PinLogic>().First());
            Logics.Add(new ClusterLogic(Config.ImageFactory,
                OnMarkerCreating, OnMarkerCreated, OnMarkerDeleting, OnMarkerDeleted));
        }

        #endregion
    }
}
