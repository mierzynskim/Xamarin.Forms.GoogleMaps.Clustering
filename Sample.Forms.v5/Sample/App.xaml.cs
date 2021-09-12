using System;
using Xamarin.Forms;
using Xamarin.Forms.v5.Sample.Services;
using Xamarin.Forms.v5.Sample.Views;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.v5.Sample
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
