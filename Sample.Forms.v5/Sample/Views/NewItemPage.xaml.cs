using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.v5.Sample.Models;
using Xamarin.Forms.v5.Sample.ViewModels;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.v5.Sample.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}