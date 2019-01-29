using Xamarin.Forms.GoogleMaps.iOS;

namespace Xamarin.Forms.GoogleMaps.Clustering.iOS
{
    public class ClusteredMapRenderer : MapRenderer
    {
        public ClusteredMapRenderer()
        {
            _logics.Add(new ClusterLogic(Config.ImageFactory, OnMarkerCreating, OnMarkerCreated, OnMarkerDeleting, OnMarkerDeleted));
        }
    }
}