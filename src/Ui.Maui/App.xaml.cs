using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Mnemi.Ui.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new ContentPage
        {
            Content = new BlazorWebView
            {
                HostPage = "wwwroot/index.html",
                RootComponents = { new RootComponent { Selector = "#app", ComponentType = typeof(Mnemi.Ui.Components.Pages.Home) } }
            }
        };
    }
}
