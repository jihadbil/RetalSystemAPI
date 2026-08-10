using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.Views.Catalog;

namespace RetalSystemAPI.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("ProductDetailPage", typeof(ProductDetailPage));
        }
    }
}
