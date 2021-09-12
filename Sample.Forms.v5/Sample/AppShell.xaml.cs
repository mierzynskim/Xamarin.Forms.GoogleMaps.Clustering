using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.v5.Sample.ViewModels;
using Xamarin.Forms.v5.Sample.Views;

namespace Xamarin.Forms.v5.Sample
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
