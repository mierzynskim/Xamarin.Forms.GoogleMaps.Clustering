using System.Linq;
using Xamarin.Forms.GoogleMaps.iOS;
using Xamarin.Forms.GoogleMaps.Logics.iOS;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    public class ClusteredMapRenderer : MapRenderer
    {
        public ClusteredMapRenderer()
        {
            _logics.Remove(_logics.OfType<PinLogic>().First());
            _logics.Add(new ClusterLogic(Config.ImageFactory,
                OnMarkerCreating, OnMarkerCreated, OnMarkerDeleting, OnMarkerDeleted));
        }
    }
}