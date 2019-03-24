using System.Linq;
using Xamarin.Forms.GoogleMaps.iOS;
using Xamarin.Forms.GoogleMaps.Logics.iOS;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    public class ClusteredMapRenderer : MapRenderer
    {
        public ClusteredMapRenderer()
        {
            Logics.Remove(Logics.OfType<PinLogic>().First());
            Logics.Add(new ClusterLogic(Config.ImageFactory,
                OnMarkerCreating, OnMarkerCreated, OnMarkerDeleting, OnMarkerDeleted));
        }
    }
}