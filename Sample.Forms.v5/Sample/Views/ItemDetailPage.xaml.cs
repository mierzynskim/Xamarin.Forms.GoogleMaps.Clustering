using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.v5.Sample.ViewModels;

namespace Xamarin.Forms.v5.Sample.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}