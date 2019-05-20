using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Clustering;

namespace XFGoogleMapSample
{
    public partial class ClusterPage
    {
        private const int ClusterItemsCount = 1000;
        private const double Extent = 0.2;
        
        private readonly Position currentPosition = new Position(52.225665764, 21.003833318);
        private readonly Random random = new Random();

        public ClusterPage()
        {
            InitializeComponent();

            Map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMeters(100)));
            
            for (var i = 0; i <= ClusterItemsCount; i++)
            {
                var lat = currentPosition.Latitude + Extent * GetRandomNumber(-1.0, 1.0);
                var lng = currentPosition.Longitude + Extent * GetRandomNumber(-1.0, 1.0);

                Map.Pins.Add(new Pin()
                {
                    Position = new Position(lat, lng),
                    Label = $"Item {i}",
                    Icon = BitmapDescriptorFactory.FromBundle("image01.png")
                });
            }

            Map.PinClicked += MapOnPinClicked;
            Map.ClusterClicked += MapOnClusterClicked;
            Map.InfoWindowClicked += MapOnInfoWindowClicked;
            Map.InfoWindowLongClicked += MapOnInfoWindowLongClicked;

            Map.Cluster();
        }

        private async void MapOnClusterClicked(object sender, ClusterClickedEventArgs e)
        {
            await DisplayAlert("Cluster clicked", $"{e.ItemsCount} pins \n{e.Position.Latitude} {e.Position.Longitude}", "Cancel");
        }

        private async void MapOnPinClicked(object sender, PinClickedEventArgs e)
        {
            await DisplayAlert("Pin clicked", e.Pin?.Label, "Cancel");
        }

        private async void MapOnInfoWindowClicked(object sender, InfoWindowClickedEventArgs e)
        {
            await DisplayAlert("Info clicked", $"{e.Pin?.Position.Latitude} {e.Pin?.Position.Longitude}", "Cancel");
        }

        private async void MapOnInfoWindowLongClicked(object sender, InfoWindowLongClickedEventArgs e)
        {
            await DisplayAlert("Info long clicked", $"{e.Pin?.Position.Latitude} {e.Pin?.Position.Longitude}", "Cancel");
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}