using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace XFGoogleMapSample
{
    public partial class ClusterPage
    {
        private const int ClusterItemsCount = 1000;// 10000;
        private const double Extent = 0.2;
        private Position _currentPosition = new Position(31.768319, 35.213710);
        private readonly Random _random = new Random();

        public ClusterPage()
        {
            InitializeComponent();

            this.Map.MoveToRegion(MapSpan.FromCenterAndRadius(this._currentPosition, Distance.FromMeters(100)));
            
            for (var i = 0; i <= ClusterItemsCount; i++)
            {
                var lat = this._currentPosition.Latitude + Extent * GetRandomNumber(-1.0, 1.0);
                var lng = this._currentPosition.Longitude + Extent * GetRandomNumber(-1.0, 1.0);

                this.Map.ClusteredPins.Add(new Pin()
                {
                    Position = new Position(lat, lng),
                    Label = $"Item {i}",
                    Icon = BitmapDescriptorFactory.FromBundle("image01.png")
                });
            }

            Map.PinClicked += MapOnPinClicked;
            //Map.CameraIdled += MapOnCameraIdled;
            Map.InfoWindowClicked += MapOnInfoWindowClicked;
            Map.InfoWindowLongClicked += MapOnInfoWindowLongClicked;

            this.Map.Cluster();
        }

        private async void MapOnPinClicked(object sender, PinClickedEventArgs e)
        {
            await DisplayAlert("Pin clicked", e.Pin?.Label, "Cancel");
        }

        private async void MapOnCameraIdled(object sender, CameraIdledEventArgs e)
        {
            await DisplayAlert("Camera idled", $"{e.Position.Target.Latitude} {e.Position.Target.Longitude}", "Cancel");
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
            return _random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}