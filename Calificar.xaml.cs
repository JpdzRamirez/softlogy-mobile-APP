using System.Reflection;

namespace TaxistasMaui;

public partial class Calificar : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    HttpClient clientehttp;
    int cal, servicio;
    Assembly asembly = typeof(Calificar).Assembly;

    public Calificar(int servicio)
    {
        InitializeComponent();
        this.servicio = servicio;
        clientehttp = new HttpClient(httpHandler);

        star1.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star2.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star3.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star4.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star5.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
    }

    private void star1_Clicked(object sender, EventArgs e)
    {
        star1.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star2.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star3.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star4.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star5.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        cal = 1;
    }

    private void star2_Clicked(object sender, EventArgs e)
    {
        star1.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star2.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star3.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star4.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star5.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        cal = 2;
    }

    private void star3_Clicked(object sender, EventArgs e)
    {
        star1.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star2.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star3.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star4.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        star5.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        cal = 3;
    }

    private void star4_Clicked(object sender, EventArgs e)
    {
        star1.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star2.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star3.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star4.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star5.Source = ImageSource.FromResource("TaxistasMaui.Recursos.stargris.png", asembly);
        cal = 4;
    }

    private void star5_Clicked(object sender, EventArgs e)
    {
        star1.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star2.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star3.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star4.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        star5.Source = ImageSource.FromResource("TaxistasMaui.Recursos.star.png", asembly);
        cal = 5;
    }

    private async void btnenviar_Clicked(object sender, EventArgs e)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
         {
                new KeyValuePair<string, string>("servicio", servicio.ToString()),
                new KeyValuePair<string, string>("puntaje", cal.ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
             });

            await clientehttp.PostAsync(App.url + "aplicaciones/taxista/calificar", content);

            ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
        }
        catch (Exception)
        {
            ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
        }
    }
}